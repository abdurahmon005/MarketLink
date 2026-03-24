using MarketLink.Application.Models.File;
using MarketLink.Application.Options;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace MarketLink.Application.Service.Impl
{
    public class MinioFileService : IFileService
    {
        private readonly IMinioClient        _minio;
        private readonly MinioOptions        _opts;
        private readonly ILogger<MinioFileService> _logger;

        // Fayl tipi → (bucket, prefix, isPublic)
        private static readonly Dictionary<FileType, (string Prefix, bool IsPublic)> FileConfig = new()
        {
            [FileType.Logo]         = ("logos",        isPublic: true),
            [FileType.Certificate]  = ("certificates", isPublic: false),
            [FileType.ProductImage] = ("products",     isPublic: true),
            [FileType.Other]        = ("other",        isPublic: false),
        };

        // Ruxsat etilgan MIME turlari
        private static readonly HashSet<string> AllowedImages = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };

        private static readonly HashSet<string> AllowedDocs = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf", "image/jpeg", "image/png"
        };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public MinioFileService(
            IMinioClient minio,
            IOptions<MinioOptions> opts,
            ILogger<MinioFileService> logger)
        {
            _minio  = minio;
            _opts   = opts.Value;
            _logger = logger;
        }

        // ──────────────── UPLOAD ────────────────

        public async Task<FileUploadResponse> UploadAsync(
            IFormFile file,
            FileType  fileType,
            Guid      ownerId,
            CancellationToken ct = default)
        {
            ValidateFile(file, fileType);

            var (prefix, isPublic) = FileConfig[fileType];
            var bucket     = isPublic ? _opts.PublicBucket : _opts.PrivateBucket;
            var ext        = Path.GetExtension(file.FileName).ToLower();
            var objectName = $"{prefix}/{ownerId}/{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}{ext}";
            var objectPath = $"{bucket}/{objectName}";

            await EnsureBucketExistsAsync(bucket, isPublic, ct);

            using var stream = file.OpenReadStream();

            var putArgs = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType);

            await _minio.PutObjectAsync(putArgs, ct);

            _logger.LogInformation("Fayl yuklandi: {Path}", objectPath);

            var response = new FileUploadResponse
            {
                ObjectPath  = objectPath,
                FileName    = file.FileName,
                FileSize    = file.Length,
                ContentType = file.ContentType,
                FileType    = fileType
            };

            if (isPublic)
            {
                response.Url = GetPublicUrl(objectPath);
            }
            else
            {
                response.PresignedUrl = await GetPresignedUrlAsync(objectPath, expirySeconds: 3600, ct);
            }

            return response;
        }

        // ──────────────── DELETE ────────────────

        public async Task<bool> DeleteAsync(
            string    objectPath,
            FileType  fileType,
            CancellationToken ct = default)
        {
            try
            {
                var (bucket, objectName) = ParseObjectPath(objectPath);

                var removeArgs = new RemoveObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(objectName);

                await _minio.RemoveObjectAsync(removeArgs, ct);

                _logger.LogInformation("Fayl o'chirildi: {Path}", objectPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Faylni o'chirishda xatolik: {Path}", objectPath);
                return false;
            }
        }

        // ──────────────── PRESIGNED URL ────────────────

        public async Task<string> GetPresignedUrlAsync(
            string objectPath,
            int    expirySeconds = 3600,
            CancellationToken ct = default)
        {
            var (bucket, objectName) = ParseObjectPath(objectPath);

            var args = new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithExpiry(expirySeconds);

            return await _minio.PresignedGetObjectAsync(args);
        }

        // ──────────────── PUBLIC URL ────────────────

        public string GetPublicUrl(string objectPath)
        {
            var baseUrl = _opts.PublicBaseUrl?.TrimEnd('/')
                ?? $"http://{_opts.Endpoint}";

            // objectPath = "bucket/prefix/ownerId/filename"
            // URL = baseUrl/objectPath
            return $"{baseUrl}/{objectPath}";
        }

        // ──────────────── BUCKET SETUP ────────────────

        private async Task EnsureBucketExistsAsync(string bucket, bool isPublic, CancellationToken ct)
        {
            var existsArgs = new BucketExistsArgs().WithBucket(bucket);
            var exists     = await _minio.BucketExistsAsync(existsArgs, ct);

            if (!exists)
            {
                var makeArgs = new MakeBucketArgs().WithBucket(bucket);
                await _minio.MakeBucketAsync(makeArgs, ct);
                _logger.LogInformation("Bucket yaratildi: {Bucket}", bucket);

                if (isPublic)
                    await SetPublicReadPolicyAsync(bucket, ct);
            }
        }

        private async Task SetPublicReadPolicyAsync(string bucket, CancellationToken ct)
        {
            var policy = $$"""
                {
                  "Version": "2012-10-17",
                  "Statement": [
                    {
                      "Effect": "Allow",
                      "Principal": {"AWS": ["*"]},
                      "Action": ["s3:GetObject"],
                      "Resource": ["arn:aws:s3:::{{bucket}}/*"]
                    }
                  ]
                }
                """;

            var args = new SetPolicyArgs()
                .WithBucket(bucket)
                .WithPolicy(policy);

            await _minio.SetPolicyAsync(args, ct);
            _logger.LogInformation("Public read policy o'rnatildi: {Bucket}", bucket);
        }

        // ──────────────── VALIDATION ────────────────

        private static void ValidateFile(IFormFile file, FileType fileType)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Fayl bo'sh");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException($"Fayl hajmi {MaxFileSizeBytes / 1024 / 1024} MB dan oshmasligi kerak");

            var allowed = fileType == FileType.Certificate ? AllowedDocs : AllowedImages;
            if (!allowed.Contains(file.ContentType))
                throw new ArgumentException($"Ruxsat etilmagan fayl turi: {file.ContentType}");
        }

        private static (string Bucket, string ObjectName) ParseObjectPath(string objectPath)
        {
            // objectPath = "bucket/prefix/ownerId/filename"
            var idx = objectPath.IndexOf('/');
            if (idx < 0)
                throw new ArgumentException($"Noto'g'ri objectPath: {objectPath}");

            return (objectPath[..idx], objectPath[(idx + 1)..]);
        }
    }
}

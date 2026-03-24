using MarketLink.Application.Models.File;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace MarketLink.Application.Service
{
    public interface IFileService
    {
        /// <summary>
        /// Faylni MinIO ga yuklaydi.
        /// Logo/ProductImage → public bucket, to'g'ridan URL qaytaradi.
        /// Certificate       → private bucket, presigned URL qaytaradi.
        /// </summary>
        Task<FileUploadResponse> UploadAsync(
            IFormFile file,
            FileType  fileType,
            Guid      ownerId,
            CancellationToken ct = default);

        /// <summary>Faylni o'chiradi</summary>
        Task<bool> DeleteAsync(
            string    objectPath,
            FileType  fileType,
            CancellationToken ct = default);

        /// <summary>Private fayl uchun muddatli presigned URL olish</summary>
        Task<string> GetPresignedUrlAsync(
            string objectPath,
            int    expirySeconds = 3600,
            CancellationToken ct = default);

        /// <summary>Public fayl URL ini qaytaradi</summary>
        string GetPublicUrl(string objectPath);
    }
}

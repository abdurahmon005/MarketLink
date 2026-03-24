using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.File
{
    public class FileUploadResponse
    {
        /// <summary>Faylni o'chirish/yangilash uchun ishlatiladi (bucket/prefix/filename)</summary>
        public string ObjectPath { get; set; } = default!;

        /// <summary>Logo va mahsulot rasmlari uchun to'g'ridan-to'g'ri URL</summary>
        public string? Url { get; set; }

        /// <summary>Sertifikat kabi private fayllar uchun presigned URL (muddatli)</summary>
        public string? PresignedUrl { get; set; }

        public string FileName  { get; set; } = default!;
        public long   FileSize  { get; set; }
        public string ContentType { get; set; } = default!;
        public FileType FileType  { get; set; }
    }
}

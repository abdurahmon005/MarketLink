namespace MarketLink.Application.Options
{
    public class MinioOptions
    {
        public const string Section = "Minio";

        public string Endpoint      { get; set; } = "localhost:9000";
        public string AccessKey     { get; set; } = "minioadmin";
        public string SecretKey     { get; set; } = "minioadmin";
        public bool   UseSSL        { get; set; } = false;
        public string PublicBucket  { get; set; } = "marketlink-public";
        public string PrivateBucket { get; set; } = "marketlink-private";

        /// <summary>Tashqi URL (nginx/proxy orqali) — agar bo'sh bo'lsa Endpoint ishlatiladi</summary>
        public string? PublicBaseUrl { get; set; }
    }
}

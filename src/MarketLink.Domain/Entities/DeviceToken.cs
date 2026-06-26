using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class DeviceToken
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public Platform Platform { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
    }
}

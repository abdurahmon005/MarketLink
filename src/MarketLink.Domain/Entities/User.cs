using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsPhoneVerified { get; set; }
        public string PasswordHash { get; set; }
        public string? Salt { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Phone verification
        public string? PhoneVerificationToken { get; set; }
        public DateTime? PhoneVerificationExpiry { get; set; }

        // Password reset
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }

        public Company? Company { get; set; }
        public Shop? Shop { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

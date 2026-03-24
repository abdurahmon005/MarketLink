using System.ComponentModel.DataAnnotations;

namespace MarketLink.Application.Models.Login
{
    public class LoginRequest
    {
        [Required] public string PhoneNumber { get; set; } = default!;
        [Required] public string Password    { get; set; } = default!;

        // Device info (optional)
        public string? DeviceId    { get; set; }
        public string? Platform    { get; set; }
        public string? DeviceModel { get; set; }
        public string? AppVersion  { get; set; }
        public string? PushToken   { get; set; }
    }
}

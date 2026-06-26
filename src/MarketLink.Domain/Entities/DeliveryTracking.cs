namespace MarketLink.Domain.Entities
{
    public class DeliveryTracking
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Guid? DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;

        /// <summary>0–100 delivery progress</summary>
        public int Progress { get; set; }

        public decimal CurrentLat { get; set; }
        public decimal CurrentLng { get; set; }
        public string CurrentLocation { get; set; } = string.Empty;

        /// <summary>Remaining distance in km</summary>
        public decimal DistanceLeft { get; set; }

        public int EtaMinutes { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Order Order { get; set; } = null!;
    }
}

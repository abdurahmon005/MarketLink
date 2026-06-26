using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Tracking
{
    public class TrackingDto
    {
        public int OrderId { get; set; }
        public int Progress { get; set; }
        public DeliveryStatus Status { get; set; }
        public string CurrentLocation { get; set; } = string.Empty;
        public decimal CurrentLat { get; set; }
        public decimal CurrentLng { get; set; }
        public decimal DistanceLeft { get; set; }
        public int EtaMinutes { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public DateTime LastUpdatedAt { get; set; }
    }

    public class ActiveDeliveryDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int EtaMinutes { get; set; }
        public int ItemCount { get; set; }
    }

    public class UpdateLocationRequest
    {
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public string CurrentLocation { get; set; } = string.Empty;
        public decimal DistanceLeft { get; set; }
        public int EtaMinutes { get; set; }
    }

    public class UpdateStatusRequest
    {
        public DeliveryStatus Status { get; set; }
        public string? Note { get; set; }
    }
}

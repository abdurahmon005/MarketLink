using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.DeviceManagement
{
    public class ActiveDeviceResponse
    {
        public Guid TokenId { get; set; }
        public string? DeviceModel { get; set; }
        public string? Platform { get; set; }
        public string? IpAddress { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCurrent { get; set; }
    }
}

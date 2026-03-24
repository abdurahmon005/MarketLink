using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Company
{
    public class CompanyProfileResponse
    {
        public int Id { get; set; }
        public string FounderName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public CompanyDirection ProductionType { get; set; }
        public string Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? SertificateUrl { get; set; }
        public double AvarageRaiting { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }



    }
}

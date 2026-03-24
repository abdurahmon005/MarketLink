using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Company
{
    public class UpdateCompanyProfileRequest
    {
        public string? FounderName { get; set; }
        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public CompanyDirection? ProductionType { get; set; }
        public string? Description { get; set; }
    }
}

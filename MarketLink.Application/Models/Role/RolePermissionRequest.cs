using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Role
{
    public class RolePermissionRequest
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
    }
}

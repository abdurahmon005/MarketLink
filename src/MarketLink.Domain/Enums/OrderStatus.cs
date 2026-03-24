using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Domain.Enums
{
    public enum OrderStatus
    {
        Pending =1,
        Accepted =2,
        Preparing=3,
        Delivered=4,
        Cancelled=5,
        Rejected=6
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Domain.Enums
{
    public enum NotificationType
    {
        NewOrder=1,
        OrderStatusChanged=2,
        OrderDelivered=3,
        OrderCancelled=4,
        NewReview=5,
        LowStock=6,
        AccountApproved=7,
        AccountRejected=8
    }
}

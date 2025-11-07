using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.Orders;

namespace api.Features.Orders.Metrics
{
     public record OrderCreationMetrics(
        string OperationId,
        string OrderTitle,
        string ISBN,
        OrderCategory Category,
        TimeSpan ValidationDuration,
        TimeSpan DatabaseSaveDuration,
        TimeSpan TotalDuration,
        bool Success,
        string? ErrorReason = null
    );
}
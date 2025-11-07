using api.Features.Orders.Metrics;
using Microsoft.Extensions.Logging;

namespace api.Common.Logging
{
    public static class LoggingExtensions
    {
        public static void LogOrderCreationMetrics(
            this ILogger logger, 
            string correlationId, 
            OrderCreationMetrics metrics)
        {
            if (logger == null || metrics == null) return;

            logger.LogInformation(
                eventId: LogEvents.OrderCreationCompleted,
                message:
                    "CorrelationId={CorrelationId} | Order Creation Metrics | OperationId={OperationId} | Title={Title} | ISBN={ISBN} | Category={Category} | " +
                    "Validation={ValidationMs}ms | DB Save={DbMs}ms | Total={TotalMs}ms | Success={Success} | Error={ErrorReason}",
                correlationId,
                metrics.OperationId,
                metrics.OrderTitle,
                metrics.ISBN,
                metrics.Category,
                metrics.ValidationDuration.TotalMilliseconds,
                metrics.DatabaseSaveDuration.TotalMilliseconds,
                metrics.TotalDuration.TotalMilliseconds,
                metrics.Success,
                metrics.ErrorReason ?? "None"
            );
        }
    }
}

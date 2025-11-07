using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using api.Features.Orders.Dtos;
using api.Features.Orders;
using api.Data;
using api.Common.Logging;
using api.Features.Orders.Metrics;

namespace api.Features.Orders.Handlers
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderProfileRequest, OrderProfileDto>
    {
        private readonly ApplicationDBContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateOrderHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private const string AllOrdersCacheKey = "all_orders";

        public CreateOrderHandler(
            ApplicationDBContext db,
            IMapper mapper,
            ILogger<CreateOrderHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache cache)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public async Task<OrderProfileDto> Handle(CreateOrderProfileRequest request, CancellationToken cancellationToken)
        {
            var correlationId = _httpContextAccessor.HttpContext?.Items["X-Correlation-ID"]?.ToString()
                                ?? Guid.NewGuid().ToString();

            var operationId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var totalStart = DateTime.UtcNow;

            _logger.LogInformation(LogEvents.OrderCreationStarted,
                "CorrelationId={CorrelationId} | OperationId={OperationId} | Order creation started | Title={Title} | ISBN={ISBN} | Category={Category}",
                correlationId, operationId, request.Title, request.ISBN, request.Category
            );

            try
            {
                var validationStart = DateTime.UtcNow;
                bool exists = await _db.Orders.AnyAsync(o => o.ISBN == request.ISBN, cancellationToken);
                var validationDuration = DateTime.UtcNow - validationStart;

                _logger.LogInformation(LogEvents.ISBNValidationPerformed,
                    "CorrelationId={CorrelationId} | OperationId={OperationId} | ISBN validation performed | Title={Title} | ISBN={ISBN} | Category={Category} | ValidationDuration={ValidationMs}ms | Exists={Exists}",
                    correlationId, operationId, request.Title, request.ISBN, request.Category, validationDuration.TotalMilliseconds, exists
                );

                if (exists)
                {
                    var failedMetrics = new OrderCreationMetrics(
                        operationId, request.Title, request.ISBN, request.Category,
                        validationDuration, TimeSpan.Zero, DateTime.UtcNow - totalStart,
                        Success: false, ErrorReason: "Duplicate ISBN"
                    );

                    _logger.LogWarning(LogEvents.OrderValidationFailed,
                        "CorrelationId={CorrelationId} | OperationId={OperationId} | Order creation failed | Title={Title} | ISBN={ISBN} | Category={Category} | Error={Error} | ValidationDuration={ValidationMs}ms",
                        correlationId, operationId, request.Title, request.ISBN, request.Category, "Duplicate ISBN", validationDuration.TotalMilliseconds
                    );

                    _logger.LogOrderCreationMetrics(correlationId, failedMetrics);
                    throw new InvalidOperationException($"Order with ISBN '{request.ISBN}' already exists.");
                }

                var order = _mapper.Map<Order>(request);

                _logger.LogInformation(LogEvents.StockValidationPerformed,
                    "CorrelationId={CorrelationId} | OperationId={OperationId} | Stock validation performed | Title={Title} | ISBN={ISBN} | Category={Category} | Stock={Stock}",
                    correlationId, operationId, request.Title, request.ISBN, request.Category, order.StockQuantity
                );

                var dbStart = DateTime.UtcNow;
                _logger.LogInformation(LogEvents.DatabaseOperationStarted,
                    "CorrelationId={CorrelationId} | OperationId={OperationId} | Saving order to database | Title={Title} | ISBN={ISBN} | Category={Category}",
                    correlationId, operationId, request.Title, request.ISBN, request.Category
                );

                _db.Orders.Add(order);
                await _db.SaveChangesAsync(cancellationToken);

                var dbSaveDuration = DateTime.UtcNow - dbStart;

                _logger.LogInformation(LogEvents.DatabaseOperationCompleted,
                    "CorrelationId={CorrelationId} | OperationId={OperationId} | Database save completed | OrderId={OrderId} | Title={Title} | ISBN={ISBN} | Category={Category} | DBSaveDuration={DbMs}ms",
                    correlationId, operationId, order.Id, request.Title, request.ISBN, request.Category, dbSaveDuration.TotalMilliseconds
                );

                _logger.LogInformation(LogEvents.CacheOperationPerformed,
                    "CorrelationId={CorrelationId} | OperationId={OperationId} | Refreshing cache for key '{CacheKey}' | Title={Title} | ISBN={ISBN} | Category={Category}",
                    correlationId, operationId, AllOrdersCacheKey, request.Title, request.ISBN, request.Category
                );

                _cache.Remove(AllOrdersCacheKey);
                var allOrders = await _db.Orders.ToListAsync(cancellationToken);
                _cache.Set(AllOrdersCacheKey, allOrders, TimeSpan.FromMinutes(5));

                _logger.LogInformation(LogEvents.CacheOperationPerformed,
                    "CorrelationId={CorrelationId} | OperationId={OperationId} | Cache refreshed for key '{CacheKey}' | Title={Title} | ISBN={ISBN} | Category={Category}",
                    correlationId, operationId, AllOrdersCacheKey, request.Title, request.ISBN, request.Category
                );

                var totalDuration = DateTime.UtcNow - totalStart;
                var successMetrics = new OrderCreationMetrics(
                    operationId, request.Title, request.ISBN, request.Category,
                    validationDuration, dbSaveDuration, totalDuration, Success: true
                );

                _logger.LogOrderCreationMetrics(correlationId, successMetrics);

                return _mapper.Map<OrderProfileDto>(order);
            }
            catch (Exception ex)
            {
                var totalDuration = DateTime.UtcNow - totalStart;
                var errorMetrics = new OrderCreationMetrics(
                    operationId, request.Title, request.ISBN, request.Category,
                    TimeSpan.Zero, TimeSpan.Zero, totalDuration, Success: false, ErrorReason: ex.Message
                );

                _logger.LogOrderCreationMetrics(correlationId, errorMetrics);
                _logger.LogError(ex,
                    "CorrelationId={CorrelationId} | OperationId={OperationId} | Unexpected error during order creation | Title={Title} | ISBN={ISBN} | Category={Category} | Error={Error}",
                    correlationId, operationId, request.Title, request.ISBN, request.Category, ex.Message
                );

                throw;
            }
        }
    }
}

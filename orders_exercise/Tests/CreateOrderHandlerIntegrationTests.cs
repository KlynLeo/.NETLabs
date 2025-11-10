using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Common.Logging;
using api.Data;
using api.Features.Orders;
using api.Features.Orders.Dtos;
using api.Features.Orders.Handlers;
using api.Features.Orders.Validators;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace api.Tests
{
    public class CreateOrderHandlerIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly ApplicationDBContext _db;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<CreateOrderHandler>> _handlerLoggerMock;
        private readonly Mock<ILogger<CreateOrderProfileValidator>> _validatorLoggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public CreateOrderHandlerIntegrationTests()
        {
            var services = new ServiceCollection();

            var dbName = $"orders_test_{Guid.NewGuid():N}";
            services.AddDbContext<ApplicationDBContext>(opts => opts.UseInMemoryDatabase(dbName));
            services.AddMemoryCache();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            _handlerLoggerMock = new Mock<ILogger<CreateOrderHandler>>();
            _validatorLoggerMock = new Mock<ILogger<CreateOrderProfileValidator>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var context = new DefaultHttpContext();
            context.Items["X-Correlation-ID"] = Guid.NewGuid().ToString();
            _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(context);

            services.AddSingleton(_ => _handlerLoggerMock.Object);
            services.AddSingleton(_ => _validatorLoggerMock.Object);
            services.AddSingleton(_ => _httpContextAccessorMock.Object);

            _provider = services.BuildServiceProvider();

            _db = _provider.GetRequiredService<ApplicationDBContext>();
            _memoryCache = _provider.GetRequiredService<IMemoryCache>();
            _mapper = _provider.GetRequiredService<IMapper>();

            _db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            try { _db.Database.EnsureDeleted(); _db.Dispose(); } catch { }
            if (_memoryCache is MemoryCache mem) mem.Dispose();
            _provider.Dispose();
        }

        private CreateOrderHandler CreateHandlerWithMocks()
            => new CreateOrderHandler(_db, _mapper, _handlerLoggerMock.Object, _httpContextAccessorMock.Object, _memoryCache);

        [Fact]
        public async Task Handle_ValidTechnicalOrderRequest_CreatesOrderWithCorrectMappings()
        {
            var handler = CreateHandlerWithMocks();
            var request = new CreateOrderProfileRequest
            {
                Title = "Introduction to Cloud Engineering",
                Author = "Jane Doe",
                ISBN = $"978{DateTime.UtcNow.Ticks % 1000000000000:D13}",
                Category = OrderCategory.Technical,
                Price = 45.50m,
                PublishedDate = DateTime.UtcNow.AddYears(-2),
                StockQuantity = 5,
                CoverImageUrl = "https://example.com/cover.jpg"
            };

            var dto = await handler.Handle(request, CancellationToken.None);

            dto.Should().NotBeNull().And.BeOfType<OrderProfileDto>();
            dto.CategoryDisplayName.Should().Be("Technical & Professional");
            dto.AuthorInitials.Should().Be("JD");
            dto.PublishedAge.Should().NotBeNullOrWhiteSpace();
            dto.PublishedAge.Any(char.IsDigit).Should().BeTrue();
            dto.FormattedPrice.Should().StartWith("$");
            dto.AvailabilityStatus.Should().NotBeNullOrWhiteSpace();

            var saved = await _db.Orders.FirstOrDefaultAsync(o => o.ISBN == request.ISBN);
            saved.Should().NotBeNull();

            _handlerLoggerMock.VerifyLog(LogLevel.Information, LogEvents.OrderCreationStarted, Times.AtLeastOnce());
        }

        [Fact]
        public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
        {
            var existing = new Order
            {
                Id = Guid.NewGuid(),
                Title = "Existing Book",
                Author = "Author X",
                ISBN = "DUPLICATE-ISBN-123",
                Category = OrderCategory.Fiction,
                Price = 10m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StockQuantity = 3,
                IsAvailable = true
            };
            await _db.Orders.AddAsync(existing);
            await _db.SaveChangesAsync();

            var handler = CreateHandlerWithMocks();
            var request = new CreateOrderProfileRequest
            {
                Title = "Another Title",
                Author = "Author Y",
                ISBN = "DUPLICATE-ISBN-123",
                Category = OrderCategory.Fiction,
                Price = 12m,
                PublishedDate = DateTime.UtcNow.AddYears(-1),
                StockQuantity = 1
            };

            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .Where(e => e is InvalidOperationException || e is FluentValidation.ValidationException)
                .WithMessage("*already exists*");

            _handlerLoggerMock.VerifyLog(LogLevel.Warning, LogEvents.OrderValidationFailed, Times.AtLeastOnce());
        }

        [Fact]
        public async Task Handle_ChildrensOrderRequest_AppliesDiscountAndConditionalMapping()
        {
            var handler = CreateHandlerWithMocks();
            var request = new CreateOrderProfileRequest
            {
                Title = "Fun Stories for Kids",
                Author = "Sally Smith",
                ISBN = $"978{DateTime.UtcNow.Ticks % 1000000000000:D13}",
                Category = OrderCategory.Children,
                Price = 40.00m,
                PublishedDate = DateTime.UtcNow.AddYears(-1),
                StockQuantity = 50,
                CoverImageUrl = "https://example.com/kids.png"
            };

            var dto = await handler.Handle(request, CancellationToken.None);

            dto.CategoryDisplayName.Should().Be("Children's Orders");
            dto.Price.Should().BeApproximately(36.00m, 0.01m);
            dto.CoverImageUrl.Should().BeNull();

            _memoryCache.TryGetValue("all_orders", out var cached).Should().BeTrue();
            cached.Should().NotBeNull();
        }
    }

    public static class LoggerMoqExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> mock, LogLevel level, int eventIdValue, Times times)
        {
            mock.Verify(x => x.Log(level,
                It.Is<EventId>(id => id.Id == eventIdValue),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                times);
        }
    }
}

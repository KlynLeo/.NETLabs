using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using api.Data;
using api.Features.Orders;
using api.Features.Orders.Dtos;

namespace api.Features.Orders.Validators
{
    public class CreateOrderProfileValidator : AbstractValidator<CreateOrderProfileRequest>
    {
        private readonly ApplicationDBContext _db;
        private readonly ILogger<CreateOrderProfileValidator> _logger;

        private static readonly string[] InappropriateWords = { "badword1", "badword2" };
        private static readonly string[] ChildrenRestrictedWords = { "violence", "drugs" };
        private static readonly string[] TechnicalKeywords = { "software", "engineering", "programming", "data", "AI", "network", "cybersecurity", "cloud" };

        public CreateOrderProfileValidator(ApplicationDBContext db, ILogger<CreateOrderProfileValidator> logger)
        {
            _db = db;
            _logger = logger;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty")
                .Length(1, 200)
                .Must(BeValidTitle).WithMessage("Title contains inappropriate words")
                .MustAsync(BeUniqueTitle).WithMessage("Title already exists for this author");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author cannot be empty")
                .Length(2, 100)
                .Must(BeValidAuthorName).WithMessage("Author contains invalid characters");

            RuleFor(x => x.ISBN)
                .NotEmpty().WithMessage("ISBN cannot be empty")
                .Must(BeValidISBN).WithMessage("ISBN format is invalid")
                .MustAsync(BeUniqueISBN).WithMessage("ISBN already exists");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Category must be a valid enum value");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThan(10000).WithMessage("Price cannot exceed $10,000");

            RuleFor(x => x.PublishedDate)
                .Must(date => date <= DateTime.UtcNow).WithMessage("Published date cannot be in the future")
                .Must(date => date.Year >= 1400).WithMessage("Published date cannot be before 1400");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
                .LessThanOrEqualTo(100000).WithMessage("Stock quantity is too large");

            RuleFor(x => x.CoverImageUrl)
                .Must(BeValidImageUrl).When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl))
                .WithMessage("CoverImageUrl must be a valid HTTP/HTTPS image URL");

            RuleFor(x => x)
                .MustAsync(PassBusinessRules)
                .WithMessage("Order violates business rules");

            When(x => x.Category == OrderCategory.Technical, () =>
            {
                RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(20)
                    .WithMessage("Technical orders must have a minimum price of $20.00");

                RuleFor(x => x.Title)
                    .Must(ContainTechnicalKeywords)
                    .WithMessage("Technical orders must contain at least one technical keyword in the title.");

                RuleFor(x => x.PublishedDate)
                    .Must(BePublishedWithinLastYears)
                    .WithMessage("Technical orders must be published within the last 5 years.");
            });

            When(x => x.Category == OrderCategory.Children, () =>
            {
                RuleFor(x => x.Price)
                    .LessThanOrEqualTo(50)
                    .WithMessage("Children's books must not cost more than $50.");

                RuleFor(x => x.Title)
                    .Must(BeAppropriateForChildren)
                    .WithMessage("Children's book titles must be appropriate for kids.");
            });

            When(x => x.Category == OrderCategory.Fiction, () =>
            {
                RuleFor(x => x.Author)
                    .MinimumLength(5)
                    .WithMessage("Fiction author names must have at least 5 characters.");
            });

            RuleFor(x => x)
                .Must(HaveLimitedStockForExpensiveOrders)
                .WithMessage("Expensive orders (>$100) must have stock ≤ 20.");

            RuleFor(x => x)
                .Must(BeRecentIfTechnical)
                .WithMessage("Technical orders must be published within the last 5 years.");
        }

        private bool BeValidTitle(string title)
        {
            _logger.LogInformation("Validating title for inappropriate words | Title={Title}", title);
            return !InappropriateWords.Any(w => title.Contains(w, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> BeUniqueTitle(CreateOrderProfileRequest request, string title, CancellationToken ct)
        {
            bool exists = await _db.Orders.AnyAsync(o => o.Title == title && o.Author == request.Author, ct);
            _logger.LogInformation("Title uniqueness check | Title={Title} | Author={Author} | Exists={Exists}", title, request.Author, exists);
            return !exists;
        }

        private bool BeValidAuthorName(string author)
        {
            return Regex.IsMatch(author, @"^[a-zA-Z\s\-'\.]+$");
        }

        private bool BeValidISBN(string isbn)
        {
            string digits = isbn.Replace("-", "").Replace(" ", "");
            return (digits.Length == 10 || digits.Length == 13) && digits.All(char.IsDigit);
        }

        private async Task<bool> BeUniqueISBN(string isbn, CancellationToken ct)
        {
            bool exists = await _db.Orders.AnyAsync(o => o.ISBN == isbn, ct);
            _logger.LogInformation("ISBN uniqueness check | ISBN={ISBN} | Exists={Exists}", isbn, exists);
            return !exists;
        }

        private bool BeValidImageUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

            string ext = System.IO.Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
            return new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(ext);
        }

        private async Task<bool> PassBusinessRules(CreateOrderProfileRequest request, CancellationToken ct)
        {
            int todayCount = await _db.Orders.CountAsync(
                o => o.CreatedAt >= DateTime.UtcNow.Date && o.CreatedAt < DateTime.UtcNow.Date.AddDays(1),
                ct);

            if (todayCount >= 500)
            {
                _logger.LogWarning("Business rule violation | Daily order limit exceeded | Count={Count}", todayCount);
                return false;
            }

            if (request.Category == OrderCategory.Technical && request.Price < 20)
            {
                _logger.LogWarning("Business rule violation | Technical order minimum price | Price={Price}", request.Price);
                return false;
            }

            if (request.Category == OrderCategory.Children &&
                ChildrenRestrictedWords.Any(w => request.Title.Contains(w, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Business rule violation | Children's order contains restricted words | Title={Title}", request.Title);
                return false;
            }

            if (request.Price > 500 && request.StockQuantity > 10)
            {
                _logger.LogWarning("Business rule violation | High-value order stock limit | Price={Price} | Stock={Stock}", request.Price, request.StockQuantity);
                return false;
            }

            return true;
        }

        private bool ContainTechnicalKeywords(string title)
        {
            return TechnicalKeywords.Any(k => title.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        private bool BeAppropriateForChildren(string title)
        {
            return !ChildrenRestrictedWords.Any(w => title.Contains(w, StringComparison.OrdinalIgnoreCase)) &&
                   !InappropriateWords.Any(w => title.Contains(w, StringComparison.OrdinalIgnoreCase));
        }

        private bool BePublishedWithinLastYears(DateTime publishedDate)
        {
            return publishedDate >= DateTime.UtcNow.AddYears(-5);
        }

        private bool HaveLimitedStockForExpensiveOrders(CreateOrderProfileRequest order)
        {
            if (order.Price > 100 && order.StockQuantity > 20)
            {
                _logger.LogWarning("Expensive order has too much stock | Price={Price} | Stock={Stock}", order.Price, order.StockQuantity);
                return false;
            }
            return true;
        }

        private bool BeRecentIfTechnical(CreateOrderProfileRequest order)
        {
            if (order.Category == OrderCategory.Technical)
                return order.PublishedDate >= DateTime.UtcNow.AddYears(-5);
            return true;
        }
    }
}

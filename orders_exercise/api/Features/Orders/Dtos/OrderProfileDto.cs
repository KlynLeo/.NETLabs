using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.Orders;

namespace api.Features.Orders.Dtos
{
    public class OrderProfileDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string ISBN { get; set; }
        public required string CategoryDisplayName { get; set; }
        public decimal  Price { get; set; }
 
        public required string FormattedPrice { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int StockQuantity { get; set; } = 0;
        public required string PublishedAge { get; set; }
        public required string AvailabilityStatus { get; set; }
        public required string AuthorInitials { get; set; }
    }
}
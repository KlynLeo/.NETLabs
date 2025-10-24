using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Enums;

namespace api.Dtos
{
    public class OrderProfileDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string CategoryDisplayName { get; set; }
        public decimal Price { get; set; }

        public string FormattedPrice { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int StockQuantity { get; set; } = 0;
        public string PublishedAge { get; set; }
        public string AvailabilityStatus { get; set; }
        public string AuthorInitials { get; set; }
    }
}
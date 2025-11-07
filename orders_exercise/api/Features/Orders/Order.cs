using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.Orders;
using System.ComponentModel.DataAnnotations;

namespace api.Features.Orders
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        public required string Title { get; set; }
        public required string Author { get; set; }

        public required string ISBN { get; set; }

        public OrderCategory Category { get; set; }

        public decimal Price { get; set; }

        public DateTime PublishedDate { get; set; }

        public string? CoverImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int StockQuantity { get; set; } = 0;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }



    }
}

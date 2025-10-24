using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Enums;
using System.ComponentModel.DataAnnotations;

namespace api.Model
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }

        public string ISBN { get; set; }

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

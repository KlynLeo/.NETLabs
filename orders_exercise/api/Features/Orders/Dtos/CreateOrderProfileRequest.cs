using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.Orders;
using api.Validators.Attributes;
using MediatR;

namespace api.Features.Orders.Dtos
{
    public class CreateOrderProfileRequest : IRequest<OrderProfileDto>
    {
        public required string Title { get; set; }
        public required string Author { get; set; }

        [ValidISBN]
        public required string ISBN { get; set; }

        [OrderCategory("Fiction", "Non-Fiction", "Science", "Biography")]
        public OrderCategory Category { get; set; }

        [PriceRange(0.01, 10000)]
        public decimal Price { get; set; }
        public DateTime PublishedDate { get; set; }
        public string? CoverImageUrl { get; set; }
        public int StockQuantity { get; set; } = 1;
    }
}
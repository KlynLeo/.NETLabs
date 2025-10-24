using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos;
using api.Model;
using AutoMapper;

namespace api.Resolvers
{
    public class AvailabilityStatusResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            if (!source.IsAvailable)
                return "Out of Stock";

            if (source.StockQuantity == 0)
                return "Unavailable";
            if (source.StockQuantity == 1)
                return "Last Copy";
            if (source.StockQuantity <= 5)
                return "Limited Stock";
            if (source.StockQuantity > 5)
                return "In Stock";

            return "Unavailable";
        }
    }
}
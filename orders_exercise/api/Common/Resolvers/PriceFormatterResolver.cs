using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.Orders.Dtos;
using api.Features.Orders;
using AutoMapper;

namespace api.Common.Resolvers
{
    public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            return source.Price.ToString("C2");
        }
    }
}
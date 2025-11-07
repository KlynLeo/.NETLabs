using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.Orders.Dtos;
using api.Features.Orders;
using AutoMapper;

namespace api.Common.Resolvers
{
    public class CategoryDisplayResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            return source.Category switch
            {
                OrderCategory.Fiction => "Fiction & Literature",
                OrderCategory.NonFiction => "Non-Fiction",
                OrderCategory.Technical => "Technical & Professional",
                OrderCategory.Children => "Children's Orders",
                _ => "Uncategorized"
            };
        }
    }
}
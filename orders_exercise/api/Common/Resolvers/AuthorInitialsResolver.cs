using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.Orders.Dtos;
using api.Features.Orders;
using AutoMapper;


namespace api.Common.Resolvers
{
    public class AuthorInitialsResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source.Author))
                return "?";

            var names = source.Author.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (names.Length == 1)
                return names[0][0].ToString().ToUpper();

            var firstInitial = names.First()[0];
            var lastInitial = names.Last()[0];
            return $"{char.ToUpper(firstInitial)}{char.ToUpper(lastInitial)}";
        }
    }
}
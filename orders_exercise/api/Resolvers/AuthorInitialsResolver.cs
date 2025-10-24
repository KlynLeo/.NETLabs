using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos;
using api.Model;
using AutoMapper;


namespace api.Resolvers
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
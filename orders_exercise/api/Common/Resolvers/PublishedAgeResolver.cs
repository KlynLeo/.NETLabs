using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Features.Orders.Dtos;
using api.Features.Orders;
using AutoMapper;



namespace api.Common.Resolvers
{
    public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
    {
        public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
        {
            var daysOld = (DateTime.UtcNow - source.PublishedDate).TotalDays;

            if (daysOld < 30)
                return "New Release";
            if (daysOld < 365)
                return $"{Math.Floor(daysOld / 30)} months old";
            if (daysOld < 1825)
                return $"{Math.Floor(daysOld / 365)} years old";
            if (Math.Abs(daysOld - 1825) < 1) 
                return "Classic";

            return $"{Math.Floor(daysOld / 365)} years old";
        }
    }
}
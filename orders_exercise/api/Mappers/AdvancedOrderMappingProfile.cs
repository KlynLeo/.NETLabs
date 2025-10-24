using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Resolvers;

using AutoMapper;
using api.Dtos;
using api.Model;

namespace api.Mappers
{
    public class AdvancedOrderMappingProfile : Profile
    {
         public AdvancedOrderMappingProfile()
        {
            CreateMap<CreateOrderProfileRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.StockQuantity > 0))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<Order, OrderProfileDto>()
                .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
                .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
                .ForMember(dest => dest.PublishedAge, opt => opt.MapFrom<PublishedAgeResolver>())
                .ForMember(dest => dest.AuthorInitials, opt => opt.MapFrom<AuthorInitialsResolver>())
                .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>());
        }
    }
}
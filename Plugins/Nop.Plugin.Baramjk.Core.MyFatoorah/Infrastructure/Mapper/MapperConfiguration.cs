using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains.PaymentFeeRule;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.PaymentFeeRule;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Infrastructure.Mapper
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<PaymentFeeRuleViewModel, PaymentFeeRule>();
            CreateMap<PaymentFeeRule, PaymentFeeRuleViewModel>();
            CreateMap<Supplier, Supplier>();
            CreateMap<Supplier, SupplierViewModel>();
            CreateMap<SupplierResponse, Supplier>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.SupplierName))
                .ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.SupplierCode))
                .ForMember(dest => dest.Mobile, opt => opt.MapFrom(src => src.Mobile))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CommissionValue, opt => opt.MapFrom(src => src.CommissionValue))
                .ForMember(dest => dest.CommissionPercentage, opt => opt.MapFrom(src => src.CommissionPercentage))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.SupplierStatus));
        }

        public int Order => 1;
    }
}
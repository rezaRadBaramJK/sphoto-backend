using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Baramjk.ContactUs.Domains;
using Nop.Plugin.Baramjk.ContactUs.Models.Api.ContactInfos;

namespace Nop.Plugin.Baramjk.ContactUs.Plugin
{
    public class ContactUsMapper : Profile, IOrderedMapperProfile
    {
        public ContactUsMapper()
        {
            CreateMap<SubmitContactInfoApiParams, ContactInfoEntity>();
        }
        
        public int Order => 1;
    }
}
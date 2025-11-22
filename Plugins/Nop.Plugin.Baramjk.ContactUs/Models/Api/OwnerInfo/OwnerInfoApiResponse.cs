using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.ContactUs.Models.Api.OwnerInfo
{
    public class OwnerInfoApiResponse: CamelCaseBaseDto
    {
        public string Email { get; set; }
        
        public string PhoneNumber { get; set; }
    }
}
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.ContactUs.Models.Api.Subject
{
    public class SubjectDto: CamelCaseModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsPayable => Price > 0;
        
        public decimal Price { get; set; }
        
        public string PriceString { get; set; }
    }
}
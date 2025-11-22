using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class CustomerAttributeValueModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors
{
    public class VendorAttributeValueModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
        
        public int DisplayOrder { get; set; }

        public int VendorAttributeId { get; set; }
    }
}
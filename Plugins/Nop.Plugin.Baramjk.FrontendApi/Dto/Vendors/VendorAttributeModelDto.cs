using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors
{
    public class VendorAttributeModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }
        
        public int DisplayOrder { get; set; }

        public int AttributeControlTypeId { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<VendorAttributeValueModelDto> Values { get; set; } = new List<VendorAttributeValueModelDto>();
    }
}
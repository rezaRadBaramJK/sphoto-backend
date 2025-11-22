using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class SpecificationAttributeWithOptionsDto
    {
        public int SpecificationAttributeId { get; set; }
        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int? SpecificationAttributeGroupId { get; set; }

        public List<SpecificationAttributeOptionModel> SpecificationAttributeOptions { get; set; }
    }
}
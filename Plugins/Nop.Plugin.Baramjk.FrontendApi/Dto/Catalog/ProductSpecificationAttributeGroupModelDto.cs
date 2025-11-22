using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ProductSpecificationAttributeGroupModelDto : ModelWithIdDto
    {
        /// <summary>
        ///     Gets or sets the specification attribute group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the specification attribute group attributes
        /// </summary>
        public IList<ProductSpecificationAttributeModelDto> Attributes { get; set; }
    }
}
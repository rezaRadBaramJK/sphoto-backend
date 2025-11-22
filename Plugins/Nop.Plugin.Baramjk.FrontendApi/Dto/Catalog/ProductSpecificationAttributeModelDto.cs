using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ProductSpecificationAttributeModelDto : ModelWithIdDto
    {
        /// <summary>
        ///     Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the values
        /// </summary>
        public IList<ProductSpecificationAttributeValueModelDto> Values { get; set; }
    }
}
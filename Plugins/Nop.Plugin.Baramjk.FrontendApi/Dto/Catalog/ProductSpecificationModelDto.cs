using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ProductSpecificationModelDto : ModelDto
    {
        /// <summary>
        ///     Gets or sets the grouped specification attribute models
        /// </summary>
        public IList<ProductSpecificationAttributeGroupModelDto> Groups { get; set; }
    }
}
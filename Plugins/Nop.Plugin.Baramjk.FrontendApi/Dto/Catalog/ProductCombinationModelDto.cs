using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ProductCombinationModelDto : ModelDto
    {
        /// <summary>
        ///     Gets or sets the attributes
        /// </summary>
        public IList<ProductAttributeModelDto> Attributes { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to the combination have stock
        /// </summary>
        public bool InStock { get; set; }
    }
}
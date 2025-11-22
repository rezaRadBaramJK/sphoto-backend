using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ProductAttributeModelDto : ModelWithIdDto
    {
        /// <summary>
        ///     Gets or sets the value IDs of the attribute
        /// </summary>
        public IList<int> ValueIds { get; set; }
    }
}
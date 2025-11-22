using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    /// <summary>
    ///     Represents a specification filter model
    /// </summary>
    public class SpecificationFilterModelDto : ModelDto
    {
        /// <summary>
        ///     Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the filtrable specification attributes
        /// </summary>
        public IList<SpecificationAttributeFilterModelDto> Attributes { get; set; }
    }
}
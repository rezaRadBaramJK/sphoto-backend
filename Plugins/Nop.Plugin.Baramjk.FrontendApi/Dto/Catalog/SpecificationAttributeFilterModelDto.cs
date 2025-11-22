using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    /// <summary>
    ///     Represents a specification attribute filter model
    /// </summary>
    public class SpecificationAttributeFilterModelDto : ModelWithIdDto
    {
        /// <summary>
        ///     Gets or sets the specification attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the values
        /// </summary>
        public IList<SpecificationAttributeValueFilterModelDto> Values { get; set; }
    }
}
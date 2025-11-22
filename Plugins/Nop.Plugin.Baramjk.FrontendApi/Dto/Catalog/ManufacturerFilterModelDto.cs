using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    /// <summary>
    ///     Represents a manufacturer filter model
    /// </summary>
    public class ManufacturerFilterModelDto : ModelDto
    {
        /// <summary>
        ///     Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the filtrable manufacturers
        /// </summary>
        public IList<SelectListItemDto> Manufacturers { get; set; }
    }
}
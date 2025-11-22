using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    /// <summary>
    ///     Represents a products price range filter model
    /// </summary>
    public class PriceRangeFilterModelDto : ModelDto
    {
        /// <summary>
        ///     Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the selected price range
        /// </summary>
        public PriceRangeModelDto SelectedPriceRange { get; set; }

        /// <summary>
        ///     Gets or sets the available price range
        /// </summary>
        public PriceRangeModelDto AvailablePriceRange { get; set; }
    }
}
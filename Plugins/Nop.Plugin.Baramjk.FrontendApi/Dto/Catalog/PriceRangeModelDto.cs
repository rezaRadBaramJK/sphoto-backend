using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    /// <summary>
    ///     Represents a price range model
    /// </summary>
    public class PriceRangeModelDto : ModelDto
    {
        /// <summary>
        ///     Gets or sets the "from" price
        /// </summary>
        public decimal? From { get; set; }

        /// <summary>
        ///     Gets or sets the "to" price
        /// </summary>
        public decimal? To { get; set; }
    }
}
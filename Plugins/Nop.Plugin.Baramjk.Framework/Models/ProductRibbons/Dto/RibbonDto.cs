using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Dto
{
    public class RibbonDto: CamelCaseModelDto
    {
        public PageProductRibbonDto CategoryPageRibbon { get; init; }

        public PageProductRibbonDto ProductPageRibbon { get; init; }
        
    }
}
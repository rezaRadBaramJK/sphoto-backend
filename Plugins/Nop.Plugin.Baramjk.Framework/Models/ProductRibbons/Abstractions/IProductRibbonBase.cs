using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Dto;

namespace Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Abstractions
{
    public interface IProductRibbonBase
    {
        ProductRibbonDto ProductRibbons { get; set; }
        
        int ProductId { get; set; }
    }
}
using Nop.Plugin.Baramjk.Framework.Models.BuyXGetY.Dto;

namespace Nop.Plugin.Baramjk.Framework.Models.BuyXGetY.Abstractions
{
    public interface IBuyXGetYBase
    {
        BuyXGetYDto BuyXGetY { get; set; }
        
        int ProductId { get; set; }
    }
}
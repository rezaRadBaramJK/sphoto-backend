using System;

namespace Nop.Plugin.Baramjk.Framework.Utils
{
    public static class PriceHelper
    {
        public static decimal CalculateDiscount(decimal oldPrice, decimal newPrice)
        {
            if (oldPrice <= 0 || newPrice <= 0)
            {
                return decimal.Zero;
            }
            decimal discount = oldPrice - newPrice;
            decimal discountPercentage = (discount / oldPrice) * 100;

            return Math.Round(discountPercentage, 2);
        }
    }
}
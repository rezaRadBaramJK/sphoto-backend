using System;

namespace Nop.Plugin.Baramjk.FrontendApi.Utils
{
    public static class PriceExtensions
    {
        public static decimal CalculateDiscount(this decimal oldPrice, decimal newPrice)
        {
            if (oldPrice <= 0 || newPrice <= 0)
            {
                return decimal.Zero;
            }

            try
            {
                // Calculate the discount
                decimal discount = oldPrice - newPrice;
                decimal discountPercentage = (discount / oldPrice) * 100;

                return Math.Round(discountPercentage, 2);
            }
            catch (Exception)
            {
                return decimal.Zero;
            }
        }
    }
}
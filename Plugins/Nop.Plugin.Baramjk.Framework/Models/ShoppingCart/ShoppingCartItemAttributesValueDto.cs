using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models.Pictures;

namespace Nop.Plugin.Baramjk.Framework.Models.ShoppingCart
{
    public class ShoppingCartItemAttributesValueDto
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string ColorSquaresRgb { get; set; }

        public List<DefaultPictureDto> Pictures { get; set; }

        public string PriceAdjustment { get; set; }

        public bool PriceAdjustmentUsePercentage { get; set; }

        public decimal PriceAdjustmentValue { get; set; }

        public bool IsPreSelected { get; set; }

        //product picture ID (associated to this value)
        public int PictureId { get; set; }

        public bool CustomerEntersQty { get; set; }

        public int Quantity { get; set; }

        public int AssociatedProductId { get; set; }
        
        public bool IsSelected { get; set; }
    }
}
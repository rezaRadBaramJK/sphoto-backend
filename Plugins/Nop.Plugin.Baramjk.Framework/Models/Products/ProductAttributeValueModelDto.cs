using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.Pictures;

namespace Nop.Plugin.Baramjk.Framework.Models.Products
{
    public class ProductAttributeValueModelDto: ModelWithIdDto
    {
        public string Name { get; set; }

        public string ColorSquaresRgb { get; set; }
        
        public DefaultPictureDto ImageSquaresPictureModel { get; set; }

        public string PriceAdjustment { get; set; }

        public bool PriceAdjustmentUsePercentage { get; set; }

        public decimal PriceAdjustmentValue { get; set; }

        public bool IsPreSelected { get; set; }
        
        public int PictureId { get; set; }

        public bool CustomerEntersQty { get; set; }

        public int Quantity { get; set; }

        public int AssociatedProductId { get; set; }
    }
}
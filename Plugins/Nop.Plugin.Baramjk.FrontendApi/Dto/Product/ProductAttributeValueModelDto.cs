using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class ProductAttributeValueModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string ColorSquaresRgb { get; set; }

        //picture model is used with "image square" attribute type
        public PictureModelDto ImageSquaresPictureModel { get; set; }

        public string PriceAdjustment { get; set; }

        public bool PriceAdjustmentUsePercentage { get; set; }

        public decimal PriceAdjustmentValue { get; set; }

        public bool IsPreSelected { get; set; }

        //product picture ID (associated to this value)
        public int PictureId { get; set; }

        public bool CustomerEntersQty { get; set; }

        public int Quantity { get; set; }

        public int AssociatedProductId { get; set; }
    }
}
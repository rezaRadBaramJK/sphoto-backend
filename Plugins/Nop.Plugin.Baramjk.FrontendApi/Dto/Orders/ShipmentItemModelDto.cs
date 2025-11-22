using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Orders
{
    public class ShipmentItemModelDto : ModelWithIdDto
    {
        public string Sku { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        public string AttributeInfo { get; set; }

        public string RentalInfo { get; set; }

        public int QuantityOrdered { get; set; }

        public int QuantityShipped { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.ScheduleDelivery.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.ScheduleDelivery.Dto;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Orders
{
    public class OrderDetailsModelDto : ModelWithIdDto, IOrderDeliveryInfo
    {
        public bool PrintMode { get; set; }

        public bool PdfInvoiceDisabled { get; set; }

        public string CustomOrderNumber { get; set; }

        public DateTime CreatedOn { get; set; }

        public string OrderStatus { get; set; }

        public bool IsReOrderAllowed { get; set; }

        public bool IsReturnRequestAllowed { get; set; }

        public bool IsShippable { get; set; }
        public bool PickupInStore { get; set; }
        public AddressModelDto PickupAddress { get; set; }
        public string ShippingStatus { get; set; }
        public AddressModelDto ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public IList<ShipmentBriefModelDto> Shipments { get; set; }

        public AddressModelDto BillingAddress { get; set; }

        public string VatNumber { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentMethodStatus { get; set; }
        public bool CanRePostProcessPayment { get; set; }
        public Dictionary<string, object> CustomValues { get; set; }

        public string OrderSubtotal { get; set; }
        public string OrderSubTotalDiscount { get; set; }
        public string OrderShipping { get; set; }
        public string PaymentMethodAdditionalFee { get; set; }
        public string CheckoutAttributeInfo { get; set; }

        public bool PricesIncludeTax { get; set; }
        public bool DisplayTaxShippingInfo { get; set; }
        public string Tax { get; set; }
        public IList<OrderDetailsTaxRateDto> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }

        public string OrderTotalDiscount { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public string RedeemedRewardPointsAmount { get; set; }
        public string OrderTotal { get; set; }

        public IList<OrderDetailsGiftCardDto> GiftCards { get; set; }

        public bool ShowSku { get; set; }
        public IList<OrderItemModelDto> Items { get; set; }

        public IList<OrderNoteDto> OrderNotes { get; set; }

        public bool ShowVendorName { get; set; }

        #region Nested Classes

        public class OrderItemModelDto : ModelWithIdDto
        {
            public Guid OrderItemGuid { get; set; }

            public string Sku { get; set; }

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public int Quantity { get; set; }

            public string AttributeInfo { get; set; }

            public string RentalInfo { get; set; }

            public string VendorName { get; set; }

            //downloadable product properties
            public int DownloadId { get; set; }

            public int LicenseId { get; set; }

            public PictureModel Picture { get; set; }
            public string ShortDescription { get; set; }
            public string FullDescription { get; set; }
        }

        public class OrderDetailsTaxRateDto : ModelDto
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public class OrderDetailsGiftCardDto : ModelDto
        {
            public string CouponCode { get; set; }
            public string Amount { get; set; }
        }

        public class OrderNoteDto : ModelWithIdDto
        {
            public bool HasDownload { get; set; }
            public string Note { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public class ShipmentBriefModelDto : ModelWithIdDto
        {
            public string TrackingNumber { get; set; }
            public DateTime? ShippedDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
        }

        #endregion

        public int OrderId => Id;
        
        public OrderDeliveryInfoDto DeliveryInfo { get; set; }
    }
}
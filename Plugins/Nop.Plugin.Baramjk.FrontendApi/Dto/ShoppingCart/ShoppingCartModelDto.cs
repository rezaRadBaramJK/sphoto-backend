using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models.Categories;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class ShoppingCartModelDto : ModelDto
    {
        public bool OnePageCheckoutEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public IList<ShoppingCartItemModelDto> Items { get; set; }

        public IList<CheckoutAttributeModelDto> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }

        public string MinOrderSubtotalWarning { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public bool TermsOfServiceOnShoppingCartPage { get; set; }

        public bool TermsOfServiceOnOrderConfirmPage { get; set; }

        public bool TermsOfServicePopup { get; set; }

        public DiscountBoxModelDto DiscountBox { get; set; }

        public GiftCardBoxModelDto GiftCardBox { get; set; }

        public OrderReviewDataModelDto OrderReviewData { get; set; }

        public IList<string> ButtonPaymentMethodViewComponentNames { get; set; }

        public bool HideCheckoutButton { get; set; }

        public bool ShowVendorName { get; set; }
        
        public decimal MinOrderSubtotalAmount { get; set; }
        
        public decimal MinOrderTotalAmount { get; set; }
    }

    public class CategorizedShoppingCartGroup
    {
        public CategoryItemDto Category { get; set; }
        public IList<ShoppingCartItemModelDto> Items { get; set; }

    }
    public class CategorizedShoppingCartModelDto : ModelDto
    {
        public bool OnePageCheckoutEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public IList<CategorizedShoppingCartGroup> Items { get; set; }

        public IList<CheckoutAttributeModelDto> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }

        public string MinOrderSubtotalWarning { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public bool TermsOfServiceOnShoppingCartPage { get; set; }

        public bool TermsOfServiceOnOrderConfirmPage { get; set; }

        public bool TermsOfServicePopup { get; set; }

        public DiscountBoxModelDto DiscountBox { get; set; }

        public GiftCardBoxModelDto GiftCardBox { get; set; }

        public OrderReviewDataModelDto OrderReviewData { get; set; }

        public IList<string> ButtonPaymentMethodViewComponentNames { get; set; }

        public bool HideCheckoutButton { get; set; }

        public bool ShowVendorName { get; set; }
    }
}
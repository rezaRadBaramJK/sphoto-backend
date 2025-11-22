using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Wishlist
{
    public class WishlistModelDto : ModelDto
    {
        public Guid CustomerGuid { get; set; }

        public string CustomerFullname { get; set; }

        public bool EmailWishlistEnabled { get; set; }

        public bool ShowSku { get; set; }

        public bool ShowProductImages { get; set; }

        public bool IsEditable { get; set; }

        public bool DisplayAddToCart { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public IList<ShoppingCartItemModel> Items { get; set; }

        public IList<string> Warnings { get; set; }

        #region Nested Classes

        public class ShoppingCartItemModel : ModelWithIdDto
        {
            public string Sku { get; set; }

            public PictureModelDto Picture { get; set; }

            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public string Discount { get; set; }

            public int? MaximumDiscountedQty { get; set; }

            public int Quantity { get; set; }

            public List<SelectListItemDto> AllowedQuantities { get; set; }

            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public string RentalInfo { get; set; }

            public bool AllowItemEditing { get; set; }

            public IList<string> Warnings { get; set; }
        }

        #endregion
    }
}
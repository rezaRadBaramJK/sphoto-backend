using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class ProductDetailsModelDto : ModelWithIdDto
    {
        public bool DefaultPictureZoomEnabled { get; set; }

        public PictureModelDto DefaultPictureModel { get; set; }

        public IList<PictureModelDto> PictureModels { get; set; }

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string SeName { get; set; }

        public bool VisibleIndividually { get; set; }

        public ProductType ProductType { get; set; }

        public bool ShowSku { get; set; }

        public string Sku { get; set; }

        public bool ShowManufacturerPartNumber { get; set; }

        public string ManufacturerPartNumber { get; set; }

        public bool ShowGtin { get; set; }

        public string Gtin { get; set; }

        public bool ShowVendor { get; set; }

        public VendorBriefInfoModelDto VendorModel { get; set; }

        public bool HasSampleDownload { get; set; }

        public GiftCardModelDto GiftCard { get; set; }

        public bool IsShipEnabled { get; set; }

        public bool IsFreeShipping { get; set; }

        public bool FreeShippingNotificationEnabled { get; set; }

        public string DeliveryDate { get; set; }

        public bool IsRental { get; set; }

        public DateTime? RentalStartDate { get; set; }

        public DateTime? RentalEndDate { get; set; }

        public DateTime? AvailableEndDate { get; set; }

        public ManageInventoryMethod ManageInventoryMethod { get; set; }

        public string StockAvailability { get; set; }

        public bool DisplayBackInStockSubscription { get; set; }

        public bool EmailAFriendEnabled { get; set; }

        public bool CompareProductsEnabled { get; set; }

        public string PageShareCode { get; set; }

        public ProductPriceModelDto ProductPrice { get; set; }

        public bool IsInWishlist { get; set; }

        public bool IsInShoppingCart { get; set; }

        public AddToCartModelDto AddToCart { get; set; }

        public ProductBreadcrumbModelDto Breadcrumb { get; set; }

        public IList<ProductTagModelDto> ProductTags { get; set; }

        public IList<ProductDetailsAttributeModelDto> ProductAttributes { get; set; }

        public ProductSpecificationModelDto ProductSpecificationModel { get; set; }

        public IList<ManufacturerBriefInfoModelDto> ProductManufacturers { get; set; }

        public ProductReviewOverviewModelDto ProductReviewOverview { get; set; }

        public ProductEstimateShippingModelDto ProductEstimateShipping { get; set; }

        public IList<TierPriceModelDto> TierPrices { get; set; }

        /// <summary>
        ///     a list of associated products. For example, "Grouped" products could have several child "simple" products
        /// </summary>
        public IList<ProductDetailsModelDto> AssociatedProducts { get; set; }

        public bool DisplayDiscontinuedMessage { get; set; }

        public string CurrentStoreName { get; set; }

        public bool InStock { get; set; }

        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }

        public int WishlistCount { get; set; }
        public int FavoriteCount { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public int OrderMaximumQuantity { get; set; }
        public int StockQuantity { get; set; }
        public int ManageInventoryMethodId { get; set; }
        public decimal ProductCost { get; set; }
        public bool Published { get; set; }
        public bool IsFavorite { get; set; }

        public List<IEntityAttachmentModel> Banners { get; set; } = new();
        
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

        #region Nested Classes

        public class ProductPriceModelDto : ModelDto
        {
            /// <summary>
            ///     The currency (in 3-letter ISO 4217 format) of the offer price
            /// </summary>
            public string CurrencyCode { get; set; }

            public string OldPrice { get; set; }

            public string Price { get; set; }

            public string PriceWithDiscount { get; set; }

            public decimal PriceValue { get; set; }

            public bool CustomerEntersPrice { get; set; }

            public bool CallForPrice { get; set; }

            public int ProductId { get; set; }

            public bool HidePrices { get; set; }

            /// <summary>
            ///     rental
            /// </summary>
            public bool IsRental { get; set; }

            public string RentalPrice { get; set; }

            /// <summary>
            ///     A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }

            /// <summary>
            ///     PAngV baseprice (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }
        }

        public class GiftCardModelDto : ModelDto
        {
            public bool IsGiftCard { get; set; }

            public string RecipientName { get; set; }

            public string RecipientEmail { get; set; }

            public string SenderName { get; set; }

            public string SenderEmail { get; set; }

            public string Message { get; set; }

            public GiftCardType GiftCardType { get; set; }
        }

        public class ProductDetailsAttributeModelDto : ModelWithIdDto
        {
            public int ProductId { get; set; }

            public int ProductAttributeId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            ///     Default value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }

            /// <summary>
            ///     Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }

            /// <summary>
            ///     Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }

            /// <summary>
            ///     Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            /// <summary>
            ///     A value indicating whether this attribute depends on some other attribute
            /// </summary>
            public bool HasCondition { get; set; }

            /// <summary>
            ///     Allowed file extensions for customer uploaded files
            /// </summary>
            public IList<string> AllowedFileExtensions { get; set; }

            public int AttributeControlType { get; set; }

            public IList<ProductAttributeValueModelDto> Values { get; set; }
        }

        #endregion
    }
}
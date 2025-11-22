using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Dto;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class ProductOverviewModelDto : ModelWithIdDto, IProductRibbonBase
    {
        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string FullDescription { get; set; }

        public string SeName { get; set; }

        public string Sku { get; set; }
        public bool IsFavorite { get; set; }

        public ProductType ProductType { get; set; }

        public bool MarkAsNew { get; set; }

        public bool IsInWishlist { get; set; }

        public bool IsInShoppingCart { get; set; }

        public bool HasAttribute { get; set; }

        public bool HasRequiredAttribute { get; set; }

        public ProductOverviewProductPriceModelDto ProductPrice { get; set; }

        public PictureModelDto DefaultPictureModel { get; set; }

        public ProductSpecificationModelDto ProductSpecificationModel { get; set; }

        public ProductReviewOverviewModelDto ReviewOverviewModel { get; set; }

        public IList<ProductTagModelDto> ProductTags { get; set; }
        
        public VendorBriefInfoModelDto VendorBrief { get; set; }

        public List<IEntityAttachmentModel> Banners { get; set; } = new();

        #region Nested Classes

        public class ProductOverviewProductPriceModelDto : ModelDto
        {
            public string OldPrice { get; set; }

            public string Price { get; set; }

            public decimal PriceValue { get; set; }

            /// <summary>
            ///     PAngV base price (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }

            public bool DisableBuyButton { get; set; }

            public bool DisableWishlistButton { get; set; }

            public bool DisableAddToCompareListButton { get; set; }

            public bool AvailableForPreOrder { get; set; }

            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }

            public bool IsRental { get; set; }

            public bool ForceRedirectionAfterAddingToCart { get; set; }

            /// <summary>
            ///     A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
        }

        #endregion

        public ProductRibbonDto ProductRibbons { get; set; } = new();

        public int ProductId
        {
            set {}
            get => Id;
        }
    }
}
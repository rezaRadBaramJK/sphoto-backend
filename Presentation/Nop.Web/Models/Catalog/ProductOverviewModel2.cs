using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial record ProductOverviewModel2 : BaseNopEntityModel
    {
        public ProductOverviewModel2()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            ProductSpecificationModel = new ProductSpecificationModel();
            ReviewOverviewModel = new ProductReviewOverviewModel();
            Categories = new List<CategoryItemModel>();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }

        public string Sku { get; set; }

        public ProductType ProductType { get; set; }

        public bool MarkAsNew { get; set; }

        public bool IsInWishlist { get; set; }

        public bool IsInShoppingCart { get; set; }
        
        public bool IsFavorite { get; set; }

        public bool HasAttribute { get; set; }

        public bool HasRequiredAttribute { get; set; }

        public int VendorId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        //price
        public ProductPriceModel ProductPrice { get; set; }

        //picture
        public PictureModel DefaultPictureModel { get; set; }
        //specification attributes

        public ProductSpecificationModel ProductSpecificationModel { get; set; }

        //price
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }

        public IList<ProductTag> ProductTags { get; set; }

        public IList<CategoryItemModel> Categories { get; set; }
        public VendorItemDto Vendor { get; set; }



        #region Nested Classes

        public partial record ProductPriceModel : BaseNopModel
        {
            public string OldPrice { get; set; }
            public string Price { get; set; }
            public decimal PriceValue { get; set; }

            /// <summary>
            /// PAngV baseprice (used in Germany)
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
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
        }

        public class VendorItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}
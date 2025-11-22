using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Models.BuyXGetY.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.BuyXGetY.Dto;
using Nop.Plugin.Baramjk.Framework.Models.Categories;
using Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Dto;
using Nop.Plugin.Baramjk.Framework.Models.Vendors;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Framework.Models.Products
{
    public record ProductOverviewDto : IProductRibbonBase, IBuyXGetYBase, ICustomProperties
    {
        public ProductOverviewDto()
        {
            ProductPrice = new ProductOverviewModel.ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            ProductSpecificationModel = new ProductSpecificationModel();
            ReviewOverviewModel = new ProductReviewOverviewModel();
            ProductAttributes = new List<ProductDetailsAttributeModelDto>();
        }

        public int Id { get; set; }
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
        
        public bool IsOutOfStock { get; set; }
        
        public bool HasAttribute { get; set; }
        public bool HasRequiredAttribute { get; set; }
        public int VendorId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public List<int> CustomerRoleIds { get; set; }
        public DateTime? AvailableStartDateTimeUtc { get; set; }
        public DateTime? AvailableEndDateTimeUtc { get; set; }
        public bool IsApproved { get; set; }
        public int VisitCount { get; set; }
        public int WishlistCount { get; set; }
        public int FavoriteCount { get; set; }

        public IList<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
        public IList<CategoryItemDto> Categories { get; set; } = new List<CategoryItemDto>();
        public VendorItemDto Vendor { get; set; }
        public ProductOverviewModel.ProductPriceModel ProductPrice { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public ProductSpecificationModel ProductSpecificationModel { get; set; }
        
        public IList<ProductDetailsAttributeModelDto> ProductAttributes { get; set; }
        public ProductReviewOverviewModel ReviewOverviewModel { get; set; }

        public BuyXGetYDto BuyXGetY { get; set; } = new();

        public ProductRibbonDto ProductRibbons { get; set; } = new();

        public int ProductId
        {
            get => Id;
            set {}
        }

        public Dictionary<string, object> CustomProperties { get; set; } = new();
    }
}
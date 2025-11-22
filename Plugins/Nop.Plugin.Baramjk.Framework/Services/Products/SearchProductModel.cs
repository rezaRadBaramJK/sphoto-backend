using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public record SearchProductModel
    {
        private int _pageNumber;
        public string KeyWord { get; set; }
        public bool SearchInAllLanguage { get; set; } = true;
        public PriceRangeModel Price { get; set; }
        public List<int> ManufacturerIds { get; set; }
        public List<int> CategoriesIds { get; set; }
        public IList<SpecificationAttributeOptionDto> FilteredSpecOptions { get; set; }
        public ProductSortingEnum OrderBy { get; set; } = ProductSortingEnum.CreatedOn;
        public string CategoryName { get; set; }
        public int VendorId { get; set; } = 0;
        public bool SubCategories { get; set; }
        public int ProductTagId { get; set; } = 0;
        public bool PrepareSpecificationAttributes { get; set; }
        public bool SearchInSku { get; set; }
        public int StoreId { get; set; } = 0;
        public bool? Publish { get; set; }
        public ProductType? ProductType { get; set; }
        public bool ShowHidden { get; set; }
        public DateTime? CreatedFromUtc { get; set; }
        public DateTime? CreatedToUtc { get; set; }
        public List<int> AttributeIds { get; set; }
        public List<int> ProductTagIds { get; set; }
        public List<int> ExcludeProductTagIds { get; set; }
        public List<int> ExcludeCategoryIds { get; set; }
        public List<int> ExcludeSpecificationAttributeOptions { get; set; }
        public List<int> ExcludeAttributeIds { get; set; }
        public List<int> ProductIds { get; set; }

        public int PageNumber
        {
            get => Math.Max(0, _pageNumber);
            set => _pageNumber = value;
        }

        public int PageSize { get; set; } = 50;
    }
}
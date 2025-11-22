using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Banners;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string SeName { get; set; }

        public int ParentCategoryId { get; set; }

        public PictureModel PictureModel { get; set; }

        public IList<CategoryDto> SubCategories { get; set; }

        public IList<ProductOverviewDto> FeaturedProducts { get; set; }

        public IList<ProductOverviewDto> Products { get; set; }
        public IList<BannerDto> Banners { get; set; } = new List<BannerDto>();
    }
}
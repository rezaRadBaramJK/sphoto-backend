using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Banners;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class CategoryProductsDto: CamelCaseModelWithIdDto
    {
        
        public string Name { get; set; }

        public List<ProductOverviewDto> Products { get; set; } = new();

        public List<BannerDto> Banners { get; set; } = new();

    }
}
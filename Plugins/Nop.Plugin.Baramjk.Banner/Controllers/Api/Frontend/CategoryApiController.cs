using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Banner.Factories;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Api.Frontend
{
    
    //REZA: remove
    public class CategoryApiController : BaseBaramjkApiController
    {
        private readonly BannerWebApiCatalogModelFactory _webApiCatalogModelFactory;

        public CategoryApiController(BannerWebApiCatalogModelFactory webApiCatalogModelFactory)
        {
            _webApiCatalogModelFactory = webApiCatalogModelFactory;
        }

        [HttpGet("/FrontendApi/Banner/GetHomepageCategories")]
        public virtual async Task<IActionResult> GetHomepageCategories(bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var model = await _webApiCatalogModelFactory.GetHomepageCategoriesAsync(featuredProduct,
                productCount, subCategoriesLevel);
            return ApiResponseFactory.Success(model);
        }
        
        [HttpGet("/FrontendApi/Banner/GetRootCategories")]
        public virtual async Task<IActionResult> GetRootCategories(bool featuredProduct = false,
            int productCount = 0, int subCategoriesLevel = 0)
        {
            var model = await _webApiCatalogModelFactory.GetRootCategoriesAsync(featuredProduct,
                productCount, subCategoriesLevel);
            return ApiResponseFactory.Success(model);
        }
        
    }
}
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class FavoriteProductController : BaseNopWebApiFrontendController
    {
        private readonly FavoriteProductService _favoriteProductService;
        private readonly IWorkContext _workContext;
        private readonly IDispatcherService _dispatcherService;

        public FavoriteProductController(
            FavoriteProductService favoriteProductService,
            IWorkContext workContext,
            IDispatcherService dispatcherService)
        {
            _favoriteProductService = favoriteProductService;
            _workContext = workContext;
            _dispatcherService = dispatcherService;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetFavoriteProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 9)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var enumerableProducts = await _favoriteProductService.GetFavoriteProductModelsAsync(customer.Id);
            var products = enumerableProducts.ToList();
            var productListDto = new ProductListDto(products, pageNumber,
                pageSize, products.Count);
            return ApiResponseFactory.Success(products);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Add([FromQuery] int id)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _favoriteProductService.AddAsync(customer.Id, id);
            return ApiResponseFactory.Success();
        }

        [HttpDelete]
        public virtual async Task<IActionResult> Delete([FromQuery] int id)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _favoriteProductService.DeleteAsync(customer.Id, id);
            return ApiResponseFactory.Success();
        }

        [HttpDelete]
        public virtual async Task<IActionResult> DeleteAll()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _favoriteProductService.DeleteAsync(customer.Id);
            return ApiResponseFactory.Success();
        }
    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Models.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.FrontendApi.Factories;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class FavoriteVendorController : BaseNopWebApiFrontendController
    {
        private readonly IWorkContext _workContext;
        private readonly VendorFactory _vendorFactory;
        private readonly FavoriteVendorService _favoriteVendorService;

        public FavoriteVendorController(IWorkContext workContext, VendorFactory vendorFactory,
            FavoriteVendorService favoriteVendorService)
        {
            _workContext = workContext;
            _vendorFactory = vendorFactory;
            _favoriteVendorService = favoriteVendorService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetFavoritesAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var ids = await _favoriteVendorService.GetCustomerFavoriteVendorsAsync(customer.Id);
            var vendorModels = await _vendorFactory.GetVendorsAsync(ids);
            return ApiResponseFactory.Success(vendorModels);
        }

        [HttpPost("{vendorId}")]
        [HttpPost("/FrontendApi/Vendor/Favorite/{vendorId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AddToFavorite([FromRoute] int vendorId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _favoriteVendorService.AddAsync(customer.Id, vendorId);
            return ApiResponseFactory.Success();
        }

        [HttpDelete("{vendorId}")]
        public virtual async Task<IActionResult> Delete(int vendorId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _favoriteVendorService.DeleteAsync(customer.Id, vendorId);
            return ApiResponseFactory.Success();
        }

        [HttpDelete]
        public virtual async Task<IActionResult> DeleteAll()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _favoriteVendorService.DeleteAllAsync(customer.Id);
            return ApiResponseFactory.Success();
        }
    }
}
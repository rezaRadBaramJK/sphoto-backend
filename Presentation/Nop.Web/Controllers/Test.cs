using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Factories;

namespace Nop.Web.Controllers
{
    public class Test : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFsactory;

        public Test(IProductService productService, IProductModelFactory productModelFactory)
        {
            _productService = productService;
            _productModelFsactory = productModelFactory;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            var product = await _productService.GetProductByIdAsync(1);
            await _productModelFsactory.PrepareProductOverviewModelsAsync(new[] { product });
            return Content("ok");
        }
    }
}
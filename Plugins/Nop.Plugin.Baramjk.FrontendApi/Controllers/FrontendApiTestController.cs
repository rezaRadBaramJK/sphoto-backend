using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class FrontendApiTestController : BaseNopWebApiFrontendController
    {
        private readonly ILogger _logger;
        private readonly IShoppingCartService _shoppingCartService;

        public FrontendApiTestController(ILogger logger, IShoppingCartService shoppingCartService)
        {
            _logger = logger;
            _shoppingCartService = shoppingCartService;
        }


        [HttpGet]
        public IActionResult Test()
        {
            return ApiResponseFactory.Success(_shoppingCartService.ToString());
        }
    }
}
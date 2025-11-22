using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Wallet.Factories;
using Nop.Plugin.Baramjk.Wallet.Models.Search;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.Wallet.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class HistoryAdminController : BaseBaramjkPluginController
    {
        
        private readonly IPermissionService _permissionService;
        private readonly CustomerWalletFactory _customerWalletFactory;

        public HistoryAdminController(
            IPermissionService permissionService, 
            CustomerWalletFactory customerWalletFactory)
        {
            _permissionService = permissionService;
            _customerWalletFactory = customerWalletFactory;
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync()
        {
            if (!await _permissionService.AuthorizeAsync(PermissionProvider.Wallet_MANAGEMEN))
                return AccessDeniedView();

            var model = await _customerWalletFactory.PrepareHistoriesSearchModelAsync(new HistoriesSearchModel());
            return View("Histories/List.cshtml", model);
        }
        
        [HttpPost]
        public async Task<IActionResult> HistoriesListAsync(HistoriesSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(PermissionProvider.Wallet_MANAGEMEN))
                return await AccessDeniedDataTablesJson();

            var model = await _customerWalletFactory.PrepareHistoriesListViewModelAsync(searchModel);
            return Json(model);
        }


        [HttpPost]
        public async Task<IActionResult> CustomerWalletHistoryAsync(CustomerHistorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(PermissionProvider.Wallet_MANAGEMEN))
                return await AccessDeniedDataTablesJson();

            var model = await _customerWalletFactory.PrepareHistoriesListAsync(searchModel);
            return Json(model);
        }

        


    }
}
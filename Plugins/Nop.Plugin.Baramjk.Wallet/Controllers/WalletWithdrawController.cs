using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.Wallet.Controllers
{
    [Permission(PermissionProvider.Wallet_MANAGEMEN)]
    [Area(AreaNames.Admin)]
    public class WalletWithdrawController : BaseBaramjkPluginController
    {
        private readonly WithdrawRequestService _withdrawRequestService;

        public WalletWithdrawController(WithdrawRequestService withdrawRequestService)
        {
            _withdrawRequestService = withdrawRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var withdrawRequestItems = await _withdrawRequestService.ListModelAsync();
            return View("WalletWithdraw/List.cshtml", withdrawRequestItems);
        }

        [HttpGet]
        public async Task<IActionResult> SetStatus(int id, bool status)
        {
            await _withdrawRequestService.SetStatusAsync(id, status);
            return RedirectToAction(nameof(Index));
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}
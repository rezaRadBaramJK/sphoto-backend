using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Wallet.Factories;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Wallet.Components
{
    [ViewComponent(Name = "WidgetsWallet")]
    public class WidgetsWalletViewComponent : BaramjkViewComponent
    {
        private readonly IWalletService _walletService;
        private readonly IInternalWalletService _internalWalletService;
        private readonly CustomerWalletFactory _customerWalletFactory;

        public WidgetsWalletViewComponent(
            IWalletService walletService,
            CustomerWalletFactory customerWalletFactory, IInternalWalletService internalWalletService)
        {
            _walletService = walletService;
            _customerWalletFactory = customerWalletFactory;
            _internalWalletService = internalWalletService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            try
            {
                if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
                    return View("WidgetsWalletViewComponent/MyAccountWalletMenu.cshtml");

                if (widgetZone == AdminWidgetZones.CustomerDetailsBlock)
                {
                    var customerId = 0;
                    if (additionalData is BaseNopEntityModel nopEntityModel)
                        customerId = nopEntityModel.Id;

                    if (customerId == 0)
                        return Content("");
                    
                    var walletEntities = await _internalWalletService.GetCustomerWalletsAsync(customerId);
                    var customerWalletViewModel = await _customerWalletFactory.PrepareCustomerWalletAsync(customerId, walletEntities);
                    return View("WidgetsWalletViewComponent/CustomerWalletComponent.cshtml", customerWalletViewModel);

                }
                
                // if (widgetZone == AdminWidgetZones.CustomerDetailsBlock)
                // {
                //     var result = int.TryParse(ViewContext.RouteData.Values["id"]?.ToString(), out var id);
                //     if (result == false)
                //         return Content("");
                //     var customerWallets = await _walletService.GetWalletsAsync(id);
                //     return View("WidgetsWalletViewComponent/AdminCustomerWallet.cshtml", customerWallets);
                // }
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            return Content("");
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}
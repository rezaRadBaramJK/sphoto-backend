using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Factories;
using Nop.Plugin.Baramjk.Wallet.Models;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.Wallet.Controllers
{
    [Permission(PermissionProvider.Wallet_MANAGEMEN)]
    [Area(AreaNames.Admin)]
    public class WalletAdminController : BaseBaramjkPluginAdminController
    {
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly WalletSettings _walletSettings;
        private readonly CustomerWalletFactory _customerWalletFactory;
        private readonly IWalletService _walletService;
        private readonly ILogger _logger;
        public WalletAdminController(INotificationService notificationService, ISettingService settingService,
            WalletSettings walletSettings, CustomerWalletFactory customerWalletFactory, IWalletService walletService, ILogger logger)
        {
            _notificationService = notificationService;
            _settingService = settingService;
            _walletSettings = walletSettings;
            _customerWalletFactory = customerWalletFactory;
            _walletService = walletService;
            _logger = logger;
        }
        [Permission(PermissionProvider.Wallet_MANAGEMEN)]
        [HttpGet]
        [Route("walletreport")]
        public async Task<IActionResult> Getall()
        {
            var data = await _customerWalletFactory.GetBriefWalletViewModel();
            return Ok(data);
        }
        [Permission(PermissionProvider.Wallet_MANAGEMEN)]
        [HttpGet]
        [Route("fixnegative")]
        public async Task<IActionResult> FixNegative()
        {
            var data = await _customerWalletFactory.GetBriefWalletViewModel();
            foreach (var model in data.Where(x=>x.Balance<Decimal.Zero))
            {
                
                var result =await _walletService.PerformAsync(new WalletTransactionRequest()
                {
                    CustomerId = model.CustomerId,
                    CurrencyCode = model.Currency,
                    Amount = Math.Abs(model.Balance),
                    Note = $"fix negative amount from admin for customer:{model.CustomerId} at : {DateTime.Now} with {Math.Abs(model.Balance)} {model.Currency}",
                    Type = WalletHistoryType.Deposit,
                    ExtraData = $"fix negative amount from admin for customer:{model.CustomerId} at : {DateTime.Now} with {Math.Abs(model.Balance)} {model.Currency}",
                });
                await _logger.InformationAsync($"fix negative amount from admin for customer:{model.CustomerId} at : {DateTime.Now} with {Math.Abs(model.Balance)} {model.Currency} result={result}");
            }
            return Ok("fixed");
        }
        public async Task<IActionResult> Configure() => Configure<WalletSettingModel>(_walletSettings);

        [HttpPost]
        public async Task<IActionResult> Configure(WalletSettingModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            await _settingService.SaveSettingModelAsync<WalletSettings>(model);
            _notificationService.SuccessNotification(await GetResByFullKeyAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Currencies;
using Nop.Services.Localization;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class RewardController : BaseNopWebApiFrontendController
    {
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IRewardPointService _rewardPointService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyTools _currencyTools;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;

        public RewardController(RewardPointsSettings rewardPointsSettings, IRewardPointService rewardPointService,
            IWorkContext workContext, ICurrencyTools currencyTools,
            IOrderTotalCalculationService orderTotalCalculationService, IOrderService orderService, IOrderProcessingService orderProcessingService, IStoreContext storeContext, ILocalizationService localizationService)
        {
            _rewardPointsSettings = rewardPointsSettings;
            _rewardPointService = rewardPointService;
            _workContext = workContext;
            _currencyTools = currencyTools;
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
        }


        [HttpGet("Info")]
        public async Task<IActionResult> GetInfo()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var rewardPointsBalance = await _rewardPointService.GetRewardPointsBalanceAsync(customer.Id, 0);
            var amount = await _orderTotalCalculationService.ConvertRewardPointsToAmountAsync(rewardPointsBalance);
            var primaryCurrency = await _currencyTools.GetPrimaryCurrencyAsync();

            var foo = new
            {
                Settings = _rewardPointsSettings,
                RewardPointsBalance = rewardPointsBalance,
                RewardPointsAmountInPrimaryCurrency = _currencyTools.CreateCurrencyModel(amount, primaryCurrency),
                RewardPointsAmountInCustomerCurrency =await _currencyTools.ConvertPrimaryToCustomerCurrencyAsync(amount),
            };

            return ApiResponseFactory.Success(foo);
        }
        
        [HttpPost("/FrontendApi/Reward/PayOrder")]
        public async Task<IActionResult> PayByRewardPointAsync([FromQuery]int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return ApiResponseFactory.NotFound("order not found");
            

            if (_orderProcessingService.CanMarkOrderAsPaid(order) == false)
                return ApiResponseFactory.BadRequest("order Can't Mark As Paid");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var rewardPointsBalance = await _rewardPointService.GetRewardPointsBalanceAsync(customer.Id, store.Id);
            rewardPointsBalance = _rewardPointService.GetReducedPointsBalance(rewardPointsBalance);
            var rewardPointsAmountBase = await _orderTotalCalculationService.ConvertRewardPointsToAmountAsync(rewardPointsBalance);
            if (order.OrderTotal > rewardPointsAmountBase)
                return ApiResponseFactory.BadRequest("Reward Points are not enough");

            
            var format = string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber);

            await _orderProcessingService.MarkOrderAsPaidAsync(order);
            var points = ConvertAmountToRewardPoints(order.OrderTotal);
            
            await _rewardPointService.AddRewardPointsHistoryEntryAsync(customer, points * -1, order.StoreId, format, order, order.OrderTotal);
            return ApiResponseFactory.Success(true);
        }
        
        
        protected virtual int ConvertAmountToRewardPoints(decimal amount)
        {
            var result = 0;
            if (amount <= 0)
                return 0;

            if (_rewardPointsSettings.ExchangeRate > 0)
                result = (int)Math.Ceiling(amount / _rewardPointsSettings.ExchangeRate);
            return result;
        }
        
        
    }
}
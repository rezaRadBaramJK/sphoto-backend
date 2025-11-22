using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.Framework.Events.Orders;
using Nop.Plugin.Baramjk.Framework.Services.Currencies;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Payments;

namespace Nop.Plugin.Baramjk.Wallet.EventConsume
{
    public class OrderRewardConsumer : IConsumer<OrderPaymentStatusEvent>, IConsumer<OrderCancelledEvent>
    {
        private readonly ICurrencyTools _currencyTools;
        private readonly WalletSettings _walletSettings;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;
        private readonly IWalletService _walletService;
        private readonly IInternalWalletService _internalWalletService;

        public OrderRewardConsumer(
            WalletSettings walletSettings,
            ICurrencyTools currencyTools,
            IPaymentService paymentService, ILogger logger, IWalletService walletService,
            IInternalWalletService internalWalletService)
        {
            _walletSettings = walletSettings;
            _currencyTools = currencyTools;
            _paymentService = paymentService;
            _logger = logger;
            _walletService = walletService;
            _internalWalletService = internalWalletService;
        }

        public async Task HandleEventAsync(OrderPaymentStatusEvent eventMessage)
        {
            var order = eventMessage.Entity;

            if (order.PaymentStatus != PaymentStatus.Paid)
            {
                await _logger.InformationAsync("WALLET : order status is not paid ");
                return;
            }


            // if (_walletSettings.EachOrderTotalEarn > 0 == false)
            // {
            //
            //     return;
            // }
            //
            // if (_walletSettings.EachOrderTotal > order.OrderTotal)
            // {
            //     return;
            // }

            // if (order.PaymentMethodSystemName.Equals(DefaultValue.SystemName))
            // {
            //     //Cash back only happens when payment is not done by wallet
            //     return;
            // }

            decimal earn = decimal.Zero;

            if (_walletSettings.CashBackEnabled)
            {
                // var customerValues = _paymentService.DeserializeCustomValues(order);
                // if (!customerValues.TryGetValue("useWalletPayment", out _))
                // {
                earn = (order.OrderTotal / 100) * _walletSettings.EarnFromOrderPercent;
                // }
            }
            else
            {
                earn = (order.OrderTotal / _walletSettings.EachOrderTotal) * _walletSettings.EachOrderTotalEarn;
                earn = Math.Min(_walletSettings.MaximumAmountEarnPerOrder, earn);
            }

            var result = await _currencyTools.ConvertPrimaryToAsync(earn, _walletSettings.DefaultCurrencyCode);
            earn = result.Amount;

            if (earn > 0)
            {
                await _walletService.PerformAsync(new WalletTransactionRequest
                {
                    CustomerId = order.CustomerId,
                    CurrencyCode = _walletService.DefaultCurrencyCode,
                    Amount = earn,
                    Type = WalletHistoryType.Reward,
                    OriginatedEntityId = order.Id,
                    Note = $"reward for order:{order.Id}"
                });
            }
        }

        public async Task HandleEventAsync(OrderCancelledEvent eventMessage)
        {
            await _logger.InformationAsync(
                $"Wallet order cancelled event hanler called for order {eventMessage.Order.Id}");
            var order = eventMessage.Order;
            //Order cancelled revert reduced wallet histories and rebalance the wallet
            await _internalWalletService.RevertWalletHistoryForCancelledOrder(order.CustomerId,
                order.CustomerCurrencyCode,
                order.Id, order.Id);
        }
    }
}
using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.Wallet.EventConsume
{
    public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IWalletService _walletService;
        private readonly WalletSettings _walletSettings;
        private readonly IOrderService _orderService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;

        public OrderPlacedConsumer(IWalletService walletService, WalletSettings walletSettings,
            IOrderService orderService, IGenericAttributeService genericAttributeService,
            ICustomerService customerService, ILogger logger)
        {
            _walletService = walletService;
            _walletSettings = walletSettings;
            _orderService = orderService;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _logger = logger;
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var useWallet = await _genericAttributeService.GetAttributeAsync<bool>(customer, "UseWalletCredit");
            if (!useWallet)
            {
                await _logger.InformationAsync("wallet order placed use wallet is false");
                return;
            }

            if (order.PaymentMethodSystemName == "Baramjk.Wallet")
            {
                await _logger.InformationAsync("wallet order placed payment method sys name is wallet");
                return;
            }

            if (_walletSettings.AutoUseWalletAmount > 0 == false)
            {
                await _logger.InformationAsync(
                    $"wallet order placed AutoUseWalletAmount is : {_walletSettings.AutoUseWalletAmount}");
                return;
            }

            if (_walletSettings.AutoUseWalletAmount > order.OrderTotal)
            {
                await _logger.InformationAsync(
                    $"wallet order placed AutoUseWalletAmount is bigger than order total: {_walletSettings.AutoUseWalletAmount} , {order.OrderTotal}");
            }

            var currentWalletBalance =
                await _walletService.GetAvailableAmountAsync(order.CustomerId, _walletSettings.DefaultCurrencyCode);
            var toWithdrawal = Math.Min(currentWalletBalance, order.OrderTotal);
            if (toWithdrawal > 0)
            {
                var withdrawal = await _walletService.PerformAsync(new WalletTransactionRequest
                {
                    CustomerId = order.CustomerId,
                    CurrencyCode = _walletSettings.DefaultCurrencyCode,
                    Amount = _walletSettings.AutoUseWalletAmount,
                    Type = WalletHistoryType.Withdrawal,
                    OriginatedEntityId = order.Id,
                    Note = $"order={order.Id} Withdrawal"
                });
                
                if (withdrawal == false)
                {
                    return;
                }
                // else
                // {
                //     await _genericAttributeService.SaveAttributeAsync(order,"PaidWithWalletAmount",toWithdrawal);
                // }


                // order.OrderTotal -= toWithdrawal;
                // if (withdrawal && currentWalletBalance >= order.OrderTotal)
                // {
                //     order.PaymentStatus = PaymentStatus.Paid;
                // }
                order.OrderTotal -= _walletSettings.AmountForRegistration;
                await _orderService.UpdateOrderAsync(order);
            }
        }
    }
}
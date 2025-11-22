using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Services.Addresses.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Plugins;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Baramjk.Wallet.Services
{
    public class WalletOrderProcesssingService : OrderProcessingService
    {
        private readonly WalletSettings _walletSettings;
        private readonly IWalletService _walletService;
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly IEncryptionService _encryptionService;
        private readonly IAddressService _addressService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IRewardPointService _rewardPointService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IDispatcherService _dispatcherService;
        public
            WalletOrderProcesssingService(CurrencySettings currencySettings, IAddressService addressService,
                IAffiliateService affiliateService, ICheckoutAttributeFormatter checkoutAttributeFormatter,
                ICountryService countryService, ICurrencyService currencyService,
                ICustomerActivityService customerActivityService, ICustomerService customerService,
                ICustomNumberFormatter customNumberFormatter, IDiscountService discountService,
                IEncryptionService encryptionService, IEventPublisher eventPublisher,
                IGenericAttributeService genericAttributeService, IGiftCardService giftCardService,
                ILanguageService languageService, ILocalizationService localizationService, ILogger logger,
                IOrderService orderService, IOrderTotalCalculationService orderTotalCalculationService,
                IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IPdfService pdfService,
                IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter,
                IProductAttributeFormatter productAttributeFormatter, IProductAttributeParser productAttributeParser,
                IProductService productService, IRewardPointService rewardPointService,
                IShipmentService shipmentService, IShippingService shippingService,
                IShoppingCartService shoppingCartService, IStateProvinceService stateProvinceService,
                ITaxService taxService, IVendorService vendorService, IWebHelper webHelper, IWorkContext workContext,
                IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings,
                OrderSettings orderSettings, PaymentSettings paymentSettings, RewardPointsSettings rewardPointsSettings,
                ShippingSettings shippingSettings, TaxSettings taxSettings, WalletSettings walletSettings, IWalletService walletService, IDispatcherService dispatcherService) : base(
            currencySettings, addressService, affiliateService, checkoutAttributeFormatter, countryService,
            currencyService, customerActivityService, customerService, customNumberFormatter, discountService,
            encryptionService, eventPublisher, genericAttributeService, giftCardService, languageService,
            localizationService, logger, orderService, orderTotalCalculationService, paymentPluginManager,
            paymentService, pdfService, priceCalculationService, priceFormatter, productAttributeFormatter,
            productAttributeParser, productService, rewardPointService, shipmentService, shippingService,
            shoppingCartService, stateProvinceService, taxService, vendorService, webHelper, workContext,
            workflowMessageService, localizationSettings, orderSettings, paymentSettings, rewardPointsSettings,
            shippingSettings, taxSettings)
        {
            _addressService = addressService;
            _customerService = customerService;
            _customNumberFormatter = customNumberFormatter;
            _encryptionService = encryptionService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _rewardPointService = rewardPointService;
            _webHelper = webHelper;
            _walletSettings = walletSettings;
            _walletService = walletService;
            _dispatcherService = dispatcherService;
        }
        
        

        protected override async Task<ProcessPaymentResult> GetProcessPaymentResultAsync(ProcessPaymentRequest processPaymentRequest, PlaceOrderContainer details)
        {
            var paymentRequired = true;
            var walletAvailableAmount = await _walletService.GetAvailableAmountAsync(details.Customer.Id, details.CustomerCurrencyCode);
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);

            var useWallet = await _genericAttributeService.GetAttributeAsync<bool>(customer, "UseWalletCredit");

            if (_walletSettings.ForceUseWalletCredit || useWallet)
            {
                if (walletAvailableAmount > decimal.Zero && details.OrderTotal < walletAvailableAmount)
                {
                    //we are use wallet completely and don't use payment method
                    paymentRequired = false;
                    processPaymentRequest.CustomValues.TryAdd("useWalletPayment", true);
                }
                if (walletAvailableAmount > decimal.Zero && details.OrderTotal > walletAvailableAmount)
                {
                    //we use wallet partially
                    paymentRequired = true;
                    processPaymentRequest.CustomValues.TryAdd("useWalletPayment", true);
                }
            }

            //process payment
            ProcessPaymentResult processPaymentResult;
            //check if is payment workflow required
            if (paymentRequired && await IsPaymentWorkflowRequiredAsync(details.Cart))
            {
                var paymentMethod = await _paymentPluginManager
                                        .LoadPluginBySystemNameAsync(processPaymentRequest.PaymentMethodSystemName, customer, processPaymentRequest.StoreId)
                                    ?? throw new NopException("Payment method couldn't be loaded");

                //ensure that payment method is active
                if (!_paymentPluginManager.IsPluginActive(paymentMethod))
                    throw new NopException("Payment method is not active");

                if (details.IsRecurringShoppingCart)
                {
                    //recurring cart
                    processPaymentResult = (await _paymentService.GetRecurringPaymentTypeAsync(processPaymentRequest.PaymentMethodSystemName)) switch
                    {
                        RecurringPaymentType.NotSupported => throw new NopException("Recurring payments are not supported by selected payment method"),
                        RecurringPaymentType.Manual or 
                            RecurringPaymentType.Automatic => await _paymentService.ProcessRecurringPaymentAsync(processPaymentRequest),
                        _ => throw new NopException("Not supported recurring payment type"),
                    };
                }
                else
                {
                    if (walletAvailableAmount > decimal.Zero && (_walletSettings.ForceUseWalletCredit || useWallet))
                    {
                        processPaymentRequest.OrderTotal -= walletAvailableAmount;
                    }
                     
                    //standard cart
                    processPaymentResult = await _paymentService.ProcessPaymentAsync(processPaymentRequest);
                    
                }
                    
            }
            else
            {
                //payment is not required
                processPaymentResult = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Paid };
            }
                
            return processPaymentResult;
        }

        protected override async Task<PlaceOrderContainer> PreparePlaceOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            if (customer.BillingAddressId is null)
            {
                var billingAddressService = EngineContext.Current.Resolve<IBillingAddressService>();
                if (billingAddressService != null)
                {
                    await billingAddressService.HandelAsync(customer);
                }
            }
            return await base.PreparePlaceOrderDetailsAsync(processPaymentRequest);
        }
        

        protected override async Task<Order> SaveOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest, ProcessPaymentResult processPaymentResult,
            PlaceOrderContainer details)
        {
            var order = new Order
            {
                StoreId = processPaymentRequest.StoreId,
                OrderGuid = processPaymentRequest.OrderGuid,
                CustomerId = details.Customer.Id,
                CustomerLanguageId = details.CustomerLanguage.Id,
                CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                OrderSubtotalInclTax = details.OrderSubTotalInclTax,
                OrderSubtotalExclTax = details.OrderSubTotalExclTax,
                OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
                OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
                OrderShippingInclTax = details.OrderShippingTotalInclTax,
                OrderShippingExclTax = details.OrderShippingTotalExclTax,
                PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
                PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
                TaxRates = details.TaxRates,
                OrderTax = details.OrderTaxTotal,
                OrderTotal = details.OrderTotal,
                RefundedAmount = decimal.Zero,
                OrderDiscount = details.OrderDiscountAmount,
                CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                CheckoutAttributesXml = details.CheckoutAttributesXml,
                CustomerCurrencyCode = details.CustomerCurrencyCode,
                CurrencyRate = details.CustomerCurrencyRate,
                AffiliateId = details.AffiliateId,
                OrderStatus = OrderStatus.Pending,
                AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                PaymentStatus = processPaymentResult.NewPaymentStatus,
                PaidDateUtc = null,
                PickupInStore = details.PickupInStore,
                ShippingStatus = details.ShippingStatus,
                ShippingMethod = details.ShippingMethodName,
                ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest),
                VatNumber = details.VatNumber,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty
            };

            if (details.BillingAddress is null)
            {
                var billingAddressService = EngineContext.Current.Resolve<IBillingAddressService>();
                if(billingAddressService == null)
                    throw new NopException("Billing address is not provided");

                details.BillingAddress = await billingAddressService.HandelAsync(details.Customer);
            }
                

            await _addressService.InsertAddressAsync(details.BillingAddress);
            order.BillingAddressId = details.BillingAddress.Id;

            if (details.PickupAddress != null)
            {
                await _addressService.InsertAddressAsync(details.PickupAddress);
                order.PickupAddressId = details.PickupAddress.Id;
            }

            if (details.ShippingAddress != null)
            {
                await _addressService.InsertAddressAsync(details.ShippingAddress);
                order.ShippingAddressId = details.ShippingAddress.Id;
            }

            await _orderService.InsertOrderAsync(order);

            //generate and set custom order number
            order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
            await _orderService.UpdateOrderAsync(order);

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var useWallet = await _genericAttributeService.GetAttributeAsync<bool>(customer, "UseWalletCredit");

            var withdrawFromWallet = _walletSettings.ForceUseWalletCredit || useWallet;
            if (withdrawFromWallet)
            {
                // var useWalletPayment = processPaymentRequest.CustomValues.TryGetValue("useWalletPayment", out _);

                var walletAmountToWithdrawal = details.OrderTotal;

                
                walletAmountToWithdrawal = await _walletService.GetAvailableAmountAsync(details.Customer.Id, details.CustomerCurrencyCode);
                processPaymentRequest.CustomValues.Add("isPaidPartOfPriceWithWallet", true);
                order.CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest);

                if (walletAmountToWithdrawal > 0)
                {
                    
                    var result = await _walletService.PerformAsync(new WalletTransactionRequest
                    {
                        CustomerId = details.Customer.Id,
                        CurrencyCode = details.CustomerCurrencyCode,
                        Amount = walletAmountToWithdrawal,
                        Type = WalletHistoryType.Withdrawal,
                        OriginatedEntityId = order.Id,
                        Note = $"wallet Withdrawal for order:{order.Id}"
                    });
                    if (result)
                    {
                        processPaymentRequest.CustomValues.Add("AmountPaidWithWallet", walletAmountToWithdrawal);

                        order.OrderTotal -= walletAmountToWithdrawal;
                        if (order.OrderTotal==Decimal.Zero)
                        {
                            order.PaymentStatus = PaymentStatus.Paid;
                            await MarkOrderAsPaidAsync(order);
                            processPaymentRequest.CustomValues.Add("FullyPaidWithWallet", true);

                        }
                    }
                    else
                    {
                        await _logger.ErrorAsync($"order {order.Id} paid with wallet . total : {order.OrderTotal} wallet amount : {walletAmountToWithdrawal} FAILED");
                        order.OrderTotal = order.OrderTotal > 0
                            ? order.OrderTotal + walletAmountToWithdrawal
                            : walletAmountToWithdrawal;
                        
                        order.PaymentStatus = PaymentStatus.Pending;
                    }
                    await _orderService.UpdateOrderAsync(order);
                }
            }

            //reward points history
            if (details.RedeemedRewardPointsAmount <= decimal.Zero)
                return order;

            order.RedeemedRewardPointsEntryId = await _rewardPointService.AddRewardPointsHistoryEntryAsync(details.Customer, -details.RedeemedRewardPoints, order.StoreId,
                string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber),
                order, details.RedeemedRewardPointsAmount);
            await _customerService.UpdateCustomerAsync(details.Customer);
            await _orderService.UpdateOrderAsync(order);
            
            return order;
        }

        protected override async Task MoveShoppingCartItemsToOrderItemsAsync(PlaceOrderContainer details, Order order)
        {
            await _dispatcherService.PublishAsync("PhotoPlatformMoveShoppingCartItemsToOrderItemsAsync", order);
            await base.MoveShoppingCartItemsToOrderItemsAsync(details, order);
        }
    }
}
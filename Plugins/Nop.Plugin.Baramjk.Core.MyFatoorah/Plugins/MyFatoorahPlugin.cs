using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Abstractions;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.ExecutePayment;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Cms;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins
{
    public class MyFatoorahPlugin : BaramjkPlugin, IPaymentMethod, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly MyFatoorahSettings _myFatoorahPaymentSettings;
        private readonly IWorkContext _workContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly MyFatoorahHttpClient _myFatoorahHttpClient;
        private readonly ILanguageService _languageService;
        private readonly INotificationService _notificationService;
        private readonly OrderSettings _orderSettings;
        private readonly IGiftCardService _giftCardService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IPermissionService _permissionService;
        private readonly IMyFatoorahPaymentClient _myFatoorahPaymentClient;
        private readonly ICustomerService _customerService;
        private readonly PaymentFeeRuleService _paymentFeeRuleService;
        private readonly SupplierService _supplierService;
        private readonly ILogger _logger;
        private readonly ITranslationService _translationService;
        private readonly TransactionService _transactionService;

        #endregion

        #region Constructor

        public MyFatoorahPlugin(
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            MyFatoorahSettings myFatoorahPaymentSettings,
            IWorkContext workContext,
            IHttpContextAccessor httpContextAccessor,
            IPaymentService paymentService,
            IOrderService orderService,
            IAddressService addressService,
            IProductService productService,
            OrderSettings orderSettings,
            MyFatoorahHttpClient myFatoorahHttpClient,
            ILanguageService languageService,
            INotificationService notificationService,
            IGiftCardService giftCardService,
            IRewardPointService rewardPointService,
            IPermissionService permissionService,
            IMyFatoorahPaymentClient myFatoorahPaymentClient,
            ICustomerService customerService,
            PaymentFeeRuleService paymentFeeRuleService,
            SupplierService supplierService, 
            ILogger logger,
            ITranslationService translationService, 
            TransactionService transactionService)
        {
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _myFatoorahPaymentSettings = myFatoorahPaymentSettings;
            _workContext = workContext;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
            _orderService = orderService;
            _addressService = addressService;
            _productService = productService;
            _myFatoorahHttpClient = myFatoorahHttpClient;
            _languageService = languageService;
            _notificationService = notificationService;
            _orderSettings = orderSettings;
            _giftCardService = giftCardService;
            _rewardPointService = rewardPointService;
            _permissionService = permissionService;
            _myFatoorahPaymentClient = myFatoorahPaymentClient;
            _customerService = customerService;
            _paymentFeeRuleService = paymentFeeRuleService;
            _supplierService = supplierService;
            _logger = logger;
            _translationService = translationService;
            _transactionService = transactionService;
        }

        #endregion

        #region Payment Method

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => _myFatoorahPaymentSettings.SkipPaymentInfo;

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending };

            if (string.IsNullOrEmpty(_myFatoorahPaymentSettings.MyFatoorahAccessKey) == false)
                return Task.FromResult(result);

            const string errorMsg = "Invalid API Token, Please contact MyFatoorah customer support";
            _notificationService.ErrorNotification(errorMsg);
            result.AddError(errorMsg);

            return Task.FromResult(result);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var baseUrl = _webHelper.GetStoreLocation(false);
            var orderDetailPage = $"{baseUrl}OrderDetails/{postProcessPaymentRequest.Order.Id}";

            _orderSettings.DisableOrderCompletedPage = false;

            string invoiceRequestJson;
            PaymentDataBaseResponse response;
            bool createPaymentLinkSuccess;
            string messageSummary;
            List<MyFatoorahSupplier> apiSuppliers;
            var paymentOptionId = 0;

            if (_myFatoorahPaymentSettings.SkipPaymentInfo)
            {
                var request = await BuildInvoiceRequestAsync(postProcessPaymentRequest);
                apiSuppliers = request.Suppliers;
                invoiceRequestJson = JsonConvert.SerializeObject(request);
                var paymentHttpResponse = await _myFatoorahPaymentClient.SendPaymentAsync(request);
                response = paymentHttpResponse.Body.Data;
                createPaymentLinkSuccess = paymentHttpResponse.Body.IsSuccess;
                messageSummary = paymentHttpResponse.Body.GetFullMessage(string.Empty);
            }
            else
            {
                var request = await BuildExecutePaymentInvoiceRequestAsync(postProcessPaymentRequest);
                apiSuppliers = request.Suppliers;
                paymentOptionId = request.PaymentMethodId;
                invoiceRequestJson = JsonConvert.SerializeObject(request);
                var paymentHttpResponse = await _myFatoorahPaymentClient.ExecutePaymentAsync(request);
                response = paymentHttpResponse.Data;
                createPaymentLinkSuccess = paymentHttpResponse.IsSuccess;
                messageSummary = paymentHttpResponse.GetFullMessage(string.Empty);
            }
            var order = postProcessPaymentRequest.Order;
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            
            //translation
            var createTranslationRequest = new CreateTranslationRequest
            {
                AmountToPay = postProcessPaymentRequest.Order.OrderTotal,
                CustomerId = order.CustomerId,
                GatewayName = postProcessPaymentRequest.Order.PaymentMethodSystemName,
                PaymentOptionId = paymentOptionId,
                ConsumerName = await _customerService.GetCustomerFullNameAsync(customer),
                ConsumerEntityType = nameof(Order),
                ConsumerEntityId = order.Id,
                 PaymentFeeRuleId = await GetPaymentFeeIdAsync(order.CustomerId),
                 PaymentFeeValue = order.PaymentMethodAdditionalFeeExclTax,
                ConsumerCallBack = "/Plugins/PaymentMyFatoorah/PDTHandler",
                ConsumerData = ""
            };
            var supplierShare = await GenerateSupplierShareAsync(apiSuppliers);
            var transaction = await _translationService.NewTranslationAsync(createTranslationRequest);
            await _transactionService.AddTransactionSupplierAsync(transaction.Id, supplierShare);
            
            //order note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = postProcessPaymentRequest.Order.Id,
                Note = $"My Fatoorah paymentRequest JSON: {invoiceRequestJson}",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = postProcessPaymentRequest.Order.Id,
                Note = $"My Fatoorah get payment link response - {response.ToJson()}",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            if (createPaymentLinkSuccess == false)
            {
                _notificationService.ErrorNotification(messageSummary);
                _orderSettings.DisableOrderCompletedPage = true;
                _httpContextAccessor.HttpContext?.Response.Redirect(orderDetailPage);
                return;
            }

            await _genericAttributeService
                .SaveAttributeAsync(postProcessPaymentRequest.Order, "MyFatoorahInvoiceId",
                    response.InvoiceId.ToString());
            _httpContextAccessor.HttpContext?.Response.Redirect(response.Url);
        }

        private async Task<SupplierShare[]> GenerateSupplierShareAsync(IList<MyFatoorahSupplier> apiSuppliers)
        {
            if (apiSuppliers == null || apiSuppliers.Any() == false)
                return Array.Empty<SupplierShare>();

            var nopSuppliers = await _supplierService.GetByCodesAsync(apiSuppliers.Select(s => s.SupplierCode).ToArray());

            var joinSuppliers =
                from nopSupplier in nopSuppliers
                join apiSupplier in apiSuppliers on nopSupplier.SupplierCode equals apiSupplier.SupplierCode
                select new SupplierShare
                {
                    NopSupplierId = nopSupplier.Id,
                    InvoiceShare = apiSupplier.InvoiceShare
                };

            return joinSuppliers.ToArray();
        }

        private async Task InitPaymentBaseRequest(
            PostProcessPaymentRequest postProcessPaymentRequest,
            PaymentRequestBase request)
        {
            var invoiceItems = new List<Invoiceitem>();

            var currentOrder = postProcessPaymentRequest.Order;
            var currencyRate = currentOrder.CurrencyRate;
            var orderItems = await _orderService.GetOrderItemsAsync(currentOrder.Id);
            foreach (var item in orderItems)
            {
                var unitPriceExclTaxRounded = Math.Round(item.UnitPriceExclTax, 3);
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                invoiceItems.Add(new Invoiceitem
                {
                    ItemName =  Regex.Replace(product.Name, @"[^a-zA-Z0-9]", ""),
                    Quantity = item.Quantity,
                    UnitPrice = unitPriceExclTaxRounded * currencyRate
                });
            }

            var totalSubtotalExcludeExtra = invoiceItems.Sum(x => x.Quantity * x.UnitPrice);
            var orderSubTotal = currentOrder.OrderSubtotalExclTax * currencyRate;
            var otherItems = new Dictionary<string, decimal>();
            if (totalSubtotalExcludeExtra != orderSubTotal)
                otherItems.Add(currentOrder.CheckoutAttributeDescription, orderSubTotal - totalSubtotalExcludeExtra);

            if (currentOrder.OrderShippingExclTax > 0m)
                otherItems.Add("Shipping " + currentOrder.ShippingMethod, currentOrder.OrderShippingExclTax);

            if (currentOrder.OrderTax > 0m)
                otherItems.Add("Tax", currentOrder.OrderTax);

            if (currentOrder.OrderDiscount > 0m)
                otherItems.Add("Discount", -currentOrder.OrderDiscount);


            if (currentOrder.OrderSubTotalDiscountInclTax > 0m)
                otherItems.Add("Sub Total Discount", -currentOrder.OrderSubTotalDiscountInclTax);

            if (currentOrder.PaymentMethodAdditionalFeeExclTax > 0m)
                otherItems.Add("Additional Fee", currentOrder.PaymentMethodAdditionalFeeExclTax);

            var giftCards = await _giftCardService.GetGiftCardUsageHistoryAsync(currentOrder);
            if (giftCards != null)
            {
                foreach (var gc in giftCards)
                {
                    otherItems.Add(
                        "GiftCard (" + (await _giftCardService.GetGiftCardByIdAsync(gc.GiftCardId)).GiftCardCouponCode +
                        ") ", -gc.UsedValue);
                }
            }

            var rewardPoints =
                await _rewardPointService.GetRewardPointsHistoryAsync(0, null, false, currentOrder.OrderGuid);
            if (rewardPoints != null)
            {
                foreach (var rp in rewardPoints)
                {
                    otherItems.Add($"Reward Points ({rp.Points}) ", -rp.UsedAmount);
                }
            }

            if (otherItems.Any())
            {
                invoiceItems.AddRange(otherItems.Select(x => new Invoiceitem
                {
                    ItemName =  Regex.Replace(x.Key, @"[^a-zA-Z0-9]", ""),
                    Quantity = 1,
                    UnitPrice = x.Value * currencyRate
                }));
            }

            var billingAddress = await _addressService.GetAddressByIdAsync(currentOrder.BillingAddressId);
            var orderCurrency = await _currencyService.GetCurrencyByCodeAsync(currentOrder.CustomerCurrencyCode);
            var invoiceValue = Math.Round(invoiceItems.Sum(x => x.Quantity * x.UnitPrice), 3);

            request.CustomerName = billingAddress.FirstName + " " + billingAddress.LastName;
            // request.CustomerMobile = billingAddress.PhoneNumber is { Length: > 10 } 
            //     ? "0" 
            //     : billingAddress.PhoneNumber;

            request.CustomerMobile = null;
            request.MobileCountryCode = null;
            request.CustomerEmail = billingAddress.Email;
            request.InvoiceItems = invoiceItems;
            request.InvoiceValue = invoiceValue;
            request.CustomerReference = currentOrder.Id.ToString();
            request.ExpiryDate = DateTime.Now.AddHours(1.0);

            request.UserDefinedField = currentOrder.Id.ToString();

            request.DisplayCurrencyIso = _myFatoorahPaymentSettings.DisplayCurrencyIsoAlpha == string.Empty
                ? orderCurrency.CurrencyCode
                : _myFatoorahPaymentSettings.DisplayCurrencyIsoAlpha;

            var storeUrl = _webHelper.GetStoreLocation(false).Replace("localhost", "127.0.0.0");
            request.CallBackUrl = $"{storeUrl}Plugins/PaymentMyFatoorah/PDTHandler";
            request.ErrorUrl = $"{storeUrl}Plugins/PaymentMyFatoorah/CancelOrder";
            //suppliers
            var defaultSupplier = await _supplierService.GetDefaultSupplier();
            if (defaultSupplier != null)
            {
                var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToArray();
                var supplierProducts = await _supplierService.GetProductsSupplierAsync(productIds);
                
                var withoutSupplierProducts = productIds
                    .Except(supplierProducts.Select(sp =>sp.ProductId))
                    .ToArray();

                var myFatoorahApiSuppliers = new List<MyFatoorahSupplier>();
                
                foreach (var orderItem in orderItems)
                {
                    //for default supplier
                    if (withoutSupplierProducts.Contains(orderItem.ProductId))
                    {
                        SubmitApiSupplier(myFatoorahApiSuppliers, orderItem, defaultSupplier, currencyRate);
                        continue;
                    }
                    //has supplier
                    var supplier = supplierProducts
                        .First(sp => sp.ProductId == orderItem.ProductId)
                        .Supplier;
                    SubmitApiSupplier(myFatoorahApiSuppliers, orderItem, supplier, currencyRate);
                }
                
                // if (currentOrder.OrderShippingExclTax > 0m)
                //     AddShippingToDefaultSupplier(
                //         myFatoorahApiSuppliers,
                //         defaultSupplier,
                //         currentOrder.OrderShippingExclTax,
                //         currencyRate);
                
                var defaultSupplierApi =
                    myFatoorahApiSuppliers.FirstOrDefault(s => s.SupplierCode == defaultSupplier.SupplierCode);
                
                if (defaultSupplierApi == null)
                {
                    defaultSupplierApi = new MyFatoorahSupplier
                    {
                        SupplierCode = defaultSupplier.SupplierCode,
                    };
                    myFatoorahApiSuppliers.Add(defaultSupplierApi);
                }
                
                foreach (var item in otherItems.Where(item => item.Value > 0))
                {
                    defaultSupplierApi.InvoiceShare += item.Value * currencyRate;
                }
                

                var supplierShareTotal = myFatoorahApiSuppliers.Sum(s => s.InvoiceShare);
                if (supplierShareTotal == invoiceValue)
                {
                    request.Suppliers = myFatoorahApiSuppliers;
                }
                    
                else
                {
                    await _logger.InsertLogAsync(
                        LogLevel.Error, 
                        "My Fatoorah - Calculate supplier share total failed.", 
                        $"supplierShareTotal = {supplierShareTotal}, invoiceValue = {invoiceValue}, -> {myFatoorahApiSuppliers.ToJson()}");
                }
                
            }
        }

        private void SubmitApiSupplier(
            List<MyFatoorahSupplier> result,
            OrderItem orderItem,
            Supplier candidSupplier,
            decimal currencyRate)
        {
            var apiSupplier = result.FirstOrDefault(s => s.SupplierCode == candidSupplier.SupplierCode);
            if (apiSupplier == null)
            {
                apiSupplier = new MyFatoorahSupplier
                {
                    SupplierCode = candidSupplier.SupplierCode
                };
                result.Add(apiSupplier);
            }
            apiSupplier.InvoiceShare += orderItem.Quantity * orderItem.UnitPriceExclTax * currencyRate;
        }
        
        private async Task<ExecutePaymentRequest> BuildExecutePaymentInvoiceRequestAsync(
            PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var request = new ExecutePaymentRequest();
            await InitPaymentBaseRequest(postProcessPaymentRequest, request);

            var orderLanguage =
                await _languageService.GetLanguageByIdAsync(postProcessPaymentRequest.Order.CustomerLanguageId);
            request.Language = (orderLanguage.Rtl ? "AR" : "EN");

            if (_myFatoorahPaymentSettings.SkipPaymentInfo == false)
                request.PaymentMethodId =
                    await _genericAttributeService.GetAttributeAsync<int>(await _workContext.GetCurrentCustomerAsync(),
                        "PaymentMethodId");

            return request;
        }

        private async Task<SendPaymentRequest> BuildInvoiceRequestAsync(
            PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var request = new SendPaymentRequest
            {
                NotificationOption = "LNK",
                ExpiryDate = DateTime.Now.AddHours(1)
            };
            await InitPaymentBaseRequest(postProcessPaymentRequest, request);
            return request;
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var paymentMethodId = form["PaymentMethodId"].ToString();
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _genericAttributeService.SaveAttributeAsync(customer, "PaymentMethodId", paymentMethodId);
            return new ProcessPaymentRequest();
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(result: false);
        }

        private async Task<int> GetPaymentFeeIdAsync(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            
            var customerBillingAddress =
                await _customerService.GetCustomerAddressAsync(customer.Id, customer.BillingAddressId ?? 0);
            
            var paymentMethodIdValue =
                await _genericAttributeService.GetAttributeAsync(customer,
                    DefaultValue.PAYMENT_METHOD_ID_ATTRIBUTE_NAME, 0, 0);
            
            var paymentFeeRule = await _paymentFeeRuleService.GetByMethodIdAndCountryId(paymentMethodIdValue,
                customerBillingAddress != null ? customerBillingAddress.CountryId ?? 0 : 0);

            return paymentFeeRule?.Id ?? 0;

        } 

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            var customer = await _customerService.GetCustomerByIdAsync(cart.First().CustomerId);
            
            var customerBillingAddress =
                await _customerService.GetCustomerAddressAsync(customer.Id, customer.BillingAddressId ?? 0);

            var paymentMethodIdValue =
                await _genericAttributeService.GetAttributeAsync(customer,
                    DefaultValue.PAYMENT_METHOD_ID_ATTRIBUTE_NAME, 0, 0);

            var paymentFeeRule = await _paymentFeeRuleService.GetByMethodIdAndCountryId(paymentMethodIdValue,
                customerBillingAddress != null ? customerBillingAddress.CountryId ?? 0 : 0);

            if (paymentFeeRule == null)
            {
                return await _paymentService.CalculateAdditionalFeeAsync(cart, _myFatoorahPaymentSettings.AdditionalFee,
                    _myFatoorahPaymentSettings.AdditionalFeePercentage);
            }


            var gatewayAdditionalFee = await _paymentService.CalculateAdditionalFeeAsync(cart,
                _myFatoorahPaymentSettings.AdditionalFee,
                _myFatoorahPaymentSettings.AdditionalFeePercentage);

            var paymentMethodAdditionFee = decimal.Zero;

            if (paymentFeeRule.Active)
            {
                paymentMethodAdditionFee += await _paymentService.CalculateAdditionalFeeAsync(cart,
                    paymentFeeRule.AdditionalFee,
                    paymentFeeRule.AdditionalFeePercentage);
            }

            return gatewayAdditionalFee + paymentMethodAdditionFee;
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0007: Expected O, but got Unknown
            CapturePaymentResult val = new CapturePaymentResult();
            val.Errors = new string[1] { "Capture method not supported" };
            return Task.FromResult<CapturePaymentResult>(val);
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var invoiceId =
                await _genericAttributeService.GetAttributeAsync<string>(refundPaymentRequest.Order,
                    "MyFatoorahInvoiceId");
            if (invoiceId == default)
            {
                return new RefundPaymentResult
                {
                    NewPaymentStatus = PaymentStatus.Paid,
                    Errors = new List<string>()
                        { $"MyFatoorahInvoiceId not found for order={refundPaymentRequest.Order.Id}" }
                };
            }

            var request = new SendRefundRequest
            {
                Amount = refundPaymentRequest.AmountToRefund,
                KeyType = "InvoiceId",
                Key = invoiceId,
                RefundChargeOnCustomer = true,
                ServiceChargeOnCustomer = _myFatoorahPaymentSettings.ChargeOnCustomer,
                Comment = "refunded by myfatoorah plugin"
            };
            var refundResult = await _myFatoorahHttpClient.CreateRefundAsync(request);

            var log =
                $"Status: {refundResult?.IsSuccess}, Message : {refundResult?.MessageSummary} {JsonConvert.SerializeObject(refundResult)} {JsonConvert.SerializeObject(request)}";

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = refundPaymentRequest.Order.Id,
                Note = log,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            var currentStatus = refundPaymentRequest.Order.PaymentStatus;

            if (refundResult.IsSuccess)
            {
                currentStatus = refundPaymentRequest.IsPartialRefund
                    ? PaymentStatus.PartiallyRefunded
                    : PaymentStatus.Refunded;
                _notificationService.SuccessNotification(refundResult.MessageSummary, true);

                return new RefundPaymentResult
                {
                    NewPaymentStatus = currentStatus
                };
            }
            else
            {
                return new RefundPaymentResult
                {
                    NewPaymentStatus = currentStatus,
                    Errors = new List<string>() { refundResult.MessageSummary, refundResult.Message }
                };
            }
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0007: Expected O, but got Unknown
            VoidPaymentResult val = new VoidPaymentResult();
            val.Errors = new string[1] { "Void method not supported" };
            return Task.FromResult<VoidPaymentResult>(val);
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0007: Expected O, but got Unknown
            ProcessPaymentResult val = new ProcessPaymentResult();
            val.Errors = new string[1] { "Recurring payment not supported" };
            return Task.FromResult<ProcessPaymentResult>(val);
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(
            CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0007: Expected O, but got Unknown
            CancelRecurringPaymentResult val = new CancelRecurringPaymentResult();
            val.Errors = new string[1] { "Recurring payment not supported" };
            return Task.FromResult<CancelRecurringPaymentResult>(val);
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            return Task.FromResult(result: true);
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "MyFatoorahAdmin";
            routeValues = new RouteValueDictionary
            {
                { "Namespaces", "Nop.Plugin.Payments.MyFatoorah.Controllers" },
                { "area", null }
            };
        }

        // public void GetPaymentInfoRoute(out string actionName, out string controllerName,
        //     out RouteValueDictionary routeValues)
        // {
        //     actionName = "PaymentInfo";
        //     controllerName = "MyFatoorahAdmin";
        //     routeValues = new RouteValueDictionary
        //     {
        //         { "Namespaces", "Nop.Plugin.Payments.MyFatoorah.Controllers" },
        //         { "area", null }
        //     };
        // }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult((IList<string>)new List<string>());
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.MyFatoorah.PaymentMethodDescription");
        }

        #endregion

        #region Plugin

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(GetDefaultSetting);
            await _localizationService.AddLocaleResourceAsync(LocalizationResources);
            await _permissionService.InstallPermissionsAsync(new PermissionProvider());
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<MyFatoorahSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.MyFatoorah");
            await _localizationService.DeleteLocaleResourcesAsync("Nop.Plugin.Baramjk.Core.MyFatoorah.");
        }

        private static SiteMapNode _pluginSiteMapNode;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await AuthorizeAsync(PermissionProvider.ManagementRecord) == false)
                return;

            if (_pluginSiteMapNode == null)
            {
                var nodes = new List<SiteMapNode>
                {
                    new()
                    {
                        Title = "Payment Fee Rule",
                        Visible = true,
                        IconClass = MenuUtils.IconClassSubsItem,
                        SystemName = "Baramjk.Core.MyFatoorah.PaymentFeeRule",
                        ChildNodes = new List<SiteMapNode>
                        {
                            CreateSiteMapNode(
                                controllerName: "PaymentFeeRule",
                                actionName: "List",
                                title: "List",
                                systemName: $"Baramjk.Core.MyFatoorah.PaymentFeeRule.List"),
                        }
                    },
                    CreateSiteMapNode("SupplierAdmin", "List", "Suppliers", "Baramjk.Core.MyFatoorah.Suppliers.List")
                };
                _pluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, nodes.ToArray());
            }


            rootNode.AddToBaramjkPluginsMenu(_pluginSiteMapNode);
        }

        public string GetPublicViewComponentName()
        {
            return "PaymentMyFatoorah";
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/MyFatoorahAdmin/Configure";
        }

        public bool HideInWidgetList => false;

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.ProductDetailsBlock
            });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "MyFatoorahWidgets";
        }


        public static MyFatoorahSettings GetDefaultSetting => new()
        {
            MyFatoorahUseSandbox = true,
            SkipPaymentInfo = false,
            MyFatoorahAccessKey =
                "rLtt6JWvbUHDDhsZnfpAhpYk4dxYDQkbcPTyGaKp2TYqQgG7FGZ5Th_WD53Oq8Ebz6A53njUoo1w3pjU1D4vs_ZMqFiz_j0urb_BH9Oq9VZoKFoJEDAbRZepGcQanImyYrry7Kt6MnMdgfG5jn4HngWoRdKduNNyP4kzcp3mRv7x00ahkm9LAK7ZRieg7k1PDAnBIOG3EyVSJ5kK4WLMvYr7sCwHbHcu4A5WwelxYK0GMJy37bNAarSJDFQsJ2ZvJjvMDmfWwDVFEVe_5tOomfVNt6bOg9mexbGjMrnHBnKnZR1vQbBtQieDlQepzTZMuQrSuKn-t5XZM7V6fCW7oP-uXGX-sMOajeX65JOf6XVpk29DP6ro8WTAflCDANC193yof8-f5_EYY-3hXhJj7RBXmizDpneEQDSaSz5sFk0sV5qPcARJ9zGG73vuGFyenjPPmtDtXtpx35A-BVcOSBYVIWe9kndG3nclfefjKEuZ3m4jL9Gg1h2JBvmXSMYiZtp9MR5I6pvbvylU_PP5xJFSjVTIz7IQSjcVGO41npnwIxRXNRxFOdIUHn0tjQ-7LwvEcTXyPsHXcMD8WtgBh-wxR8aKX7WPSsT1O8d8reb2aR7K3rkV3K82K_0OgawImEpwSvp9MNKynEAJQS6ZHe_J_l77652xwPNxMRTMASk1ZsJL",
            FrontendBase = "",
            FailedFrontendCallback = "",
            SuccessFrontendCallback = ""
        };

        public static readonly IDictionary<string, string> LocalizationResources = new Dictionary<string, string>
        {
            // ["Baramjk.Core.MyFatoorah"] = "My Fatoorah",
            //Settings
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.AccessKey"] = "Access Key",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.WebhookSecretKey"] = "Webhook Secret Key",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.UseSandbox"] = "Use sandbox",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.Redirect"] = "Redirect",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.FrontendRedirectBaseUrl"] =
                "Frontend redirect base URL",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.BackendRedirectBaseUrl"] = "Nop web redirect base URL",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.SuccessFrontendCallback"] = "Success frontend callback",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.FailedFrontendCallback"] = "Failed frontend callback",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.DisplayCurrencyIsoAlpha"] =
                "Display currency ISO alpha",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.AdditionalFee"] = "Additional fee",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.AdditionalFeePercentage"] =
                "Additional fee. Use percentage",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.DefaultSupplierId"] = "Default supplier",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.SkipPaymentInfo"] = "Skip payment info",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Settings.ChargeOnCustomer"] = "Charge on customer",
            //PaymentFeeRule
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.PaymentMethodName"] = "Payment Method",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.PaymentMethodId"] = "Payment Method",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.CountryName"] = "Country",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.CountryId"] = "Country",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.FeePercent"] = "Fee Percent",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.FeeAmount"] = "Fee Amount",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.FeeUsageType"] = "Fee Usage Type",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.AdditionalFee"] = "Additional Fee",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.AdditionalFeePercentage"] = "Use Percentage",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.Active"] = "Active",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.AddNew"] = "Add New Rule",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.Edit"] = "Edit Rule",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.List"] = "Payment Fee Rules",
            ["Plugins.Payments.MyFatoorah.Admin.PaymentFeeRule.BackToPaymentFeeRulesList"] =
                "Back To PaymentFee Rules List",
            //Suppliers
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers"] = "Suppliers",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Name"] = "Name",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Email"] = "Email",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Mobile"] = "Mobile",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.CommissionValue"] = "Commission Value",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.CommissionPercentage"] = "Commission Percentage",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Status"] = "Status",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.SupplierCode"] = "Supplier code",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.AddNew"] = "Add new supplier",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.BackToList"] = "Back to list",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.EditSupplier"] = "Edit supplier",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Product.Supplier"] = "Supplier",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Product.Assign"] = "Assign",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Product.AssignSupplier"] = "Assign supplier",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Product.AssignSuccessful"] =
                "The supplier has been assigned to product successfully.",
            ["Nop.Plugin.Baramjk.Core.MyFatoorah.Admin.Suppliers.Sync"] = "Sync",
        };

        #endregion
    }
}
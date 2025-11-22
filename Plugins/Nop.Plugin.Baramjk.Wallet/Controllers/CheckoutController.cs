// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Nop.Core;
// using Nop.Core.Domain.Common;
// using Nop.Core.Domain.Customers;
// using Nop.Core.Domain.Orders;
// using Nop.Core.Domain.Payments;
// using Nop.Core.Domain.Shipping;
// using Nop.Plugin.Baramjk.Wallet.Plugins;
// using Nop.Services.Catalog;
// using Nop.Services.Common;
// using Nop.Services.Customers;
// using Nop.Services.Directory;
// using Nop.Services.Localization;
// using Nop.Services.Logging;
// using Nop.Services.Orders;
// using Nop.Services.Payments;
// using Nop.Services.Shipping;
// using Nop.Web.Factories;
// using Nop.Web.Framework.Controllers;
// using Nop.Web.Models.Checkout;
//
// namespace Nop.Plugin.Baramjk.Wallet.Controllers
// {
//     public record WalletCheckoutPaymentMethodModel : CheckoutPaymentMethodModel
//     {
//         public bool DisplayUseWalletCredit { get; set; }
//         public bool UseWalletCredit { get; set; }
//         public int WalletCreditBalance { get; set; }
//     }
//
//     public class CheckoutController : Nop.Web.Controllers.CheckoutController
//     {
//         #region Fields
//
//         private readonly AddressSettings _addressSettings;
//         private readonly CustomerSettings _customerSettings;
//         private readonly IAddressAttributeParser _addressAttributeParser;
//         private readonly IAddressService _addressService;
//         private readonly ICheckoutModelFactory _checkoutModelFactory;
//         private readonly ICountryService _countryService;
//         private readonly ICustomerService _customerService;
//         private readonly IGenericAttributeService _genericAttributeService;
//         private readonly ILocalizationService _localizationService;
//         private readonly ILogger _logger;
//         private readonly IOrderProcessingService _orderProcessingService;
//         private readonly IOrderService _orderService;
//         private readonly IPaymentPluginManager _paymentPluginManager;
//         private readonly IPaymentService _paymentService;
//         private readonly IProductService _productService;
//         private readonly IShippingService _shippingService;
//         private readonly IShoppingCartService _shoppingCartService;
//         private readonly IStoreContext _storeContext;
//         private readonly IWebHelper _webHelper;
//         private readonly IWorkContext _workContext;
//         private readonly OrderSettings _orderSettings;
//         private readonly PaymentSettings _paymentSettings;
//         private readonly RewardPointsSettings _rewardPointsSettings;
//         private readonly ShippingSettings _shippingSettings;
//         private readonly WalletSettings _walletSettings;
//         #endregion
//
//         public CheckoutController(AddressSettings addressSettings, CustomerSettings customerSettings, IAddressAttributeParser addressAttributeParser, IAddressService addressService, ICheckoutModelFactory checkoutModelFactory, ICountryService countryService, ICustomerService customerService, IGenericAttributeService genericAttributeService, ILocalizationService localizationService, ILogger logger, IOrderProcessingService orderProcessingService, IOrderService orderService, IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IProductService productService, IShippingService shippingService, IShoppingCartService shoppingCartService, IStoreContext storeContext, IWebHelper webHelper, IWorkContext workContext, OrderSettings orderSettings, PaymentSettings paymentSettings, RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, WalletSettings walletSettings) : base(addressSettings, customerSettings, addressAttributeParser, addressService, checkoutModelFactory, countryService, customerService, genericAttributeService, localizationService, logger, orderProcessingService, orderService, paymentPluginManager, paymentService, productService, shippingService, shoppingCartService, storeContext, webHelper, workContext, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings)
//         {
//             _addressSettings = addressSettings;
//             _customerSettings = customerSettings;
//             _addressAttributeParser = addressAttributeParser;
//             _addressService = addressService;
//             _checkoutModelFactory = checkoutModelFactory;
//             _countryService = countryService;
//             _customerService = customerService;
//             _genericAttributeService = genericAttributeService;
//             _localizationService = localizationService;
//             _logger = logger;
//             _orderProcessingService = orderProcessingService;
//             _orderService = orderService;
//             _paymentPluginManager = paymentPluginManager;
//             _paymentService = paymentService;
//             _productService = productService;
//             _shippingService = shippingService;
//             _shoppingCartService = shoppingCartService;
//             _storeContext = storeContext;
//             _webHelper = webHelper;
//             _workContext = workContext;
//             _orderSettings = orderSettings;
//             _paymentSettings = paymentSettings;
//             _rewardPointsSettings = rewardPointsSettings;
//             _shippingSettings = shippingSettings;
//             _walletSettings = walletSettings;
//         }
//         // [HttpGet(Order = -1)]
//         // public override Task<IActionResult> Index()
//         // {
//         //     return base.Index();
//         // } 
//         // [HttpGet(Order = -1)]
//         // public override Task<IActionResult> OnePageCheckout()
//         // {
//         //     return base.OnePageCheckout();
//         // }
//
//         [HttpGet(Order = -1)]
//
//                 /// <returns>A task that represents the asynchronous operation</returns>
//         public override async Task<IActionResult> PaymentMethod()
//         {
//             //validation
//             if (_orderSettings.CheckoutDisabled)
//                 return RedirectToRoute("ShoppingCart");
//
//             var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
//
//             if (!cart.Any())
//                 return RedirectToRoute("ShoppingCart");
//
//             if (_orderSettings.OnePageCheckoutEnabled)
//                 return RedirectToRoute("CheckoutOnePage");
//
//             if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
//                 return Challenge();
//
//             //Check whether payment workflow is required
//             //we ignore reward points during cart total calculation
//             var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
//             if (!isPaymentWorkflowRequired)
//             {
//                 await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
//                     NopCustomerDefaults.SelectedPaymentMethodAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
//                 return RedirectToRoute("CheckoutPaymentInfo");
//             }
//
//             //filter by country
//             var filterByCountryId = 0;
//             if (_addressSettings.CountryEnabled)
//             {
//                 filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(await _workContext.GetCurrentCustomerAsync()))?.CountryId ?? 0;
//             }
//
//             //model
//             var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);
//
//             if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
//                 paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
//             {
//                 //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
//                 //so customer doesn't have to choose a payment method
//
//                 await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
//                     NopCustomerDefaults.SelectedPaymentMethodAttribute,
//                     paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
//                     (await _storeContext.GetCurrentStoreAsync()).Id);
//                 return RedirectToRoute("CheckoutPaymentInfo");
//             }
//
//             var walletPaymentMethodModel = (WalletCheckoutPaymentMethodModel)paymentMethodModel;
//             walletPaymentMethodModel.DisplayUseWalletCredit = !_walletSettings.ForceUseWalletCredit;
//             walletPaymentMethodModel.WalletCreditBalance = 1000;
//             
//             return View(paymentMethodModel);
//         }
//
//         [HttpPost, ActionName("PaymentMethod")]
//         [FormValueRequired("nextstep")]
//         /// <returns>A task that represents the asynchronous operation</returns>
//         public virtual async Task<IActionResult> SelectPaymentMethod(string paymentmethod, WalletCheckoutPaymentMethodModel model)
//         {
//             //validation
//             if (_orderSettings.CheckoutDisabled)
//                 return RedirectToRoute("ShoppingCart");
//
//             var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
//
//             if (!cart.Any())
//                 return RedirectToRoute("ShoppingCart");
//
//             if (_orderSettings.OnePageCheckoutEnabled)
//                 return RedirectToRoute("CheckoutOnePage");
//
//             if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
//                 return Challenge();
//
//             //reward points
//             if (_rewardPointsSettings.Enabled)
//             {
//                 await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
//                     NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, model.UseRewardPoints,
//                     (await _storeContext.GetCurrentStoreAsync()).Id);
//             }
//
//             //Check whether payment workflow is required
//             var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
//             if (!isPaymentWorkflowRequired)
//             {
//                 await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
//                     NopCustomerDefaults.SelectedPaymentMethodAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
//                 return RedirectToRoute("CheckoutPaymentInfo");
//             }
//             //payment method 
//             if (string.IsNullOrEmpty(paymentmethod))
//                 return await PaymentMethod();
//
//             if (!await _paymentPluginManager.IsPluginActiveAsync(paymentmethod, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id))
//                 return await PaymentMethod();
//
//             //save
//             await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
//                 NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentmethod, (await _storeContext.GetCurrentStoreAsync()).Id);
//
//             return RedirectToRoute("CheckoutPaymentInfo");
//         }
//
//     }
// }
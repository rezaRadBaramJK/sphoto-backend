using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Html;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Wishlist;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class WishlistController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public WishlistController(
            ICustomerService customerService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IProductService productService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            ShoppingCartSettings shoppingCartSettings, WishlistFactory wishlistFactory)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _productService = productService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _shoppingCartSettings = shoppingCartSettings;
            _wishlistFactory = wishlistFactory;
        }

        #endregion

        [HttpGet]
        [ProducesResponseType(typeof(WishlistModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SavedProductList([FromQuery] GetSavedProductRequest request)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return ApiResponseFactory.BadRequest();

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (customer == null)
                return ApiResponseFactory.BadRequest();

            var overviewModels = await _wishlistFactory.PrepareSavedProductsAsync(customer, request);

            return ApiResponseFactory.Success(overviewModels);
        }

        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly WishlistFactory _wishlistFactory;

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(typeof(WishlistModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Wishlist([FromQuery] Guid? customerGuid)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return ApiResponseFactory.BadRequest();

            var customer = customerGuid.HasValue
                ? await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
                : await _workContext.GetCurrentCustomerAsync();

            if (customer == null)
                return ApiResponseFactory.BadRequest();

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            var model = new WishlistModel();
            model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart, customerGuid.HasValue);

            return ApiResponseFactory.Success(model.ToDto<WishlistModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(WishlistModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UpdateWishlist([FromBody] IDictionary<string, string> form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return ApiResponseFactory.BadRequest("No permission to perform this operation");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            var allIdsToRemove = form.ContainsKey("removefromcart")
                ? form["removefromcart"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList()
                : new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
                else
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (int.TryParse(form[formKey], out var newQuantity))
                            {
                                var currSciWarnings = await _shoppingCartService.UpdateShoppingCartItemAsync(
                                    await _workContext.GetCurrentCustomerAsync(),
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }

                            break;
                        }
            }

            //updated wishlist
            cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = new WishlistModel();
            model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }

            return ApiResponseFactory.Success(model.ToDto<WishlistModelDto>());
        }

        [HttpPut]
        [ProducesResponseType(typeof(WishlistModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AddItemsToCartFromWishlist([FromBody] IDictionary<string, string> form,
            [FromQuery] Guid? customerGuid)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return ApiResponseFactory.BadRequest();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return ApiResponseFactory.BadRequest();

            var pageCustomer = customerGuid.HasValue
                ? await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
                : await _workContext.GetCurrentCustomerAsync();

            if (pageCustomer == null)
                return ApiResponseFactory.BadRequest();

            var pageCart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            var allWarnings = new List<string>();
            var countOfAddedItems = 0;
            var allIdsToAdd = form.ContainsKey("addtocart")
                ? form["addtocart"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)
                    .ToList()
                : new List<int>();
            foreach (var sci in pageCart)
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var product = await _productService.GetProductByIdAsync(sci.ProductId);

                    var warnings = await _shoppingCartService.AddToCartAsync(
                        await _workContext.GetCurrentCustomerAsync(),
                        product, ShoppingCartType.ShoppingCart,
                        (await _storeContext.GetCurrentStoreAsync()).Id,
                        sci.AttributesXml, sci.CustomerEnteredPrice,
                        sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity);
                    if (!warnings.Any())
                        countOfAddedItems++;
                    if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        !customerGuid.HasValue && //own wishlist
                        !warnings.Any()) //no warnings ( already in the cart)
                        //let's remove the item from wishlist
                        await _shoppingCartService.DeleteShoppingCartItemAsync(sci);

                    allWarnings.AddRange(warnings);
                }

            if (countOfAddedItems > 0)
            {
                //redirect to the shopping cart page

                if (allWarnings.Any())
                    _notificationService.ErrorNotification(
                        await _localizationService.GetResourceAsync("Wishlist.AddToCart.Error"));

                return ApiResponseFactory.BadRequest();
            }

            _notificationService.WarningNotification(
                await _localizationService.GetResourceAsync("Wishlist.AddToCart.NoAddedItems"));
            //no items added. redisplay the wishlist page

            if (allWarnings.Any())
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync("Wishlist.AddToCart.Error"));

            var cart = await _shoppingCartService.GetShoppingCartAsync(pageCustomer, ShoppingCartType.Wishlist,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            var rezModel = new WishlistModel();
            rezModel = await _shoppingCartModelFactory.PrepareWishlistModelAsync(rezModel, cart,
                !customerGuid.HasValue);

            return ApiResponseFactory.Success(rezModel.ToDto<WishlistModelDto>());
        }

        [HttpGet]
        [ProducesResponseType(typeof(WishlistEmailAFriendModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> EmailWishlist()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
                !_shoppingCartSettings.EmailWishlistEnabled)
                return ApiResponseFactory.BadRequest();

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return ApiResponseFactory.BadRequest();

            var model = new WishlistEmailAFriendModel();
            model = await _shoppingCartModelFactory.PrepareWishlistEmailAFriendModelAsync(model, false);

            return ApiResponseFactory.Success(model.ToDto<WishlistEmailAFriendModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(WishlistEmailAFriendModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> EmailWishlistSend([FromBody] WishlistEmailAFriendModelDto model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
                !_shoppingCartSettings.EmailWishlistEnabled)
                return ApiResponseFactory.BadRequest();

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.Wishlist, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return ApiResponseFactory.BadRequest();

            //check whether the current customer is guest and ia allowed to email wishlist
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_shoppingCartSettings.AllowAnonymousUsersToEmailWishlist)
                return ApiResponseFactory.BadRequest();

            //email
            await _workflowMessageService.SendWishlistEmailAFriendMessageAsync(
                await _workContext.GetCurrentCustomerAsync(),
                (await _workContext.GetWorkingLanguageAsync()).Id, model.YourEmailAddress,
                model.FriendEmail,
                HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));

            model.SuccessfullySent = true;
            model.Result = await _localizationService.GetResourceAsync("Wishlist.EmailAFriend.SuccessfullySent");

            return ApiResponseFactory.Success(model);
        }

        #endregion
    }
}
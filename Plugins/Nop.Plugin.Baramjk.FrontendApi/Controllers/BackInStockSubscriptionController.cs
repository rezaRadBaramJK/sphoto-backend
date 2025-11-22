using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.BackInStockSubscription;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class BackInStockSubscriptionController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public BackInStockSubscriptionController(CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IProductService productService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IWorkContext workContext)
        {
            _catalogSettings = catalogSettings;
            _customerSettings = customerSettings;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _customerService = customerService;
            _localizationService = localizationService;
            _productService = productService;
            _storeContext = storeContext;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
        }

        #endregion

        #region Fields

        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Methods

        /// <summary>
        ///     Product details page > back in stock subscribe
        /// </summary>
        /// <param name="productId">The product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BackInStockSubscribeModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SubscribePopup(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return ApiResponseFactory.NotFound($"No product found with the specified id={productId}");

            var model = new BackInStockSubscribeModel
            {
                ProductId = product.Id,
                ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                IsCurrentCustomerRegistered =
                    await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()),
                MaximumBackInStockSubscriptions = _catalogSettings.MaximumBackInStockSubscriptions,
                CurrentNumberOfBackInStockSubscriptions = (await _backInStockSubscriptionService
                        .GetAllSubscriptionsByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id,
                            (await _storeContext.GetCurrentStoreAsync()).Id, 0, 1))
                    .TotalCount
            };
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                await _productService.GetTotalStockQuantityAsync(product) <= 0)
            {
                //out of stock
                model.SubscriptionAllowed = true;
                model.AlreadySubscribed = await _backInStockSubscriptionService
                    .FindSubscriptionAsync((await _workContext.GetCurrentCustomerAsync()).Id, product.Id,
                        (await _storeContext.GetCurrentStoreAsync()).Id) != null;
            }

            return ApiResponseFactory.Success(model.ToDto<BackInStockSubscribeModelDto>());
        }

        /// <summary>
        ///     Back in stock subscribe
        /// </summary>
        /// <param name="productId">Product id</param>
        [HttpPost("{productId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SubscribePopupPOST(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return ApiResponseFactory.NotFound($"No product found with the specified id={productId}");

            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.Success(
                    await _localizationService.GetResourceAsync("BackInStockSubscriptions.OnlyRegistered"));

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                await _productService.GetTotalStockQuantityAsync(product) <= 0)
            {
                //out of stock
                var subscription = await _backInStockSubscriptionService
                    .FindSubscriptionAsync((await _workContext.GetCurrentCustomerAsync()).Id, product.Id,
                        (await _storeContext.GetCurrentStoreAsync()).Id);
                if (subscription != null)
                {
                    //subscription already exists
                    //unsubscribe
                    await _backInStockSubscriptionService.DeleteSubscriptionAsync(subscription);

                    return ApiResponseFactory.Success(
                        await _localizationService.GetResourceAsync(
                            "BackInStockSubscriptions.Notification.Unsubscribed"));
                }

                //subscription does not exist
                //subscribe
                if ((await _backInStockSubscriptionService
                        .GetAllSubscriptionsByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id,
                            (await _storeContext.GetCurrentStoreAsync()).Id, 0, 1))
                    .TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
                    return ApiResponseFactory.Success(string.Format(
                        await _localizationService.GetResourceAsync("BackInStockSubscriptions.MaxSubscriptions"),
                        _catalogSettings.MaximumBackInStockSubscriptions));
                subscription = new BackInStockSubscription
                {
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    ProductId = product.Id,
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _backInStockSubscriptionService.InsertSubscriptionAsync(subscription);

                return ApiResponseFactory.Success(
                    await _localizationService.GetResourceAsync("BackInStockSubscriptions.Notification.Subscribed"));
            }

            //subscription not possible
            return ApiResponseFactory.Success(
                await _localizationService.GetResourceAsync("BackInStockSubscriptions.NotAllowed"));
        }

        /// <summary>
        ///     My account / Back in stock subscriptions
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerBackInStockSubscriptionsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CustomerSubscriptions([FromQuery] int? pageNumber)
        {
            if (_customerSettings.HideBackInStockSubscriptionsTab)
                return ApiResponseFactory.BadRequest(
                    $"The setting {nameof(_customerSettings.HideBackInStockSubscriptionsTab)} is true.");

            var pageIndex = 0;
            if (pageNumber > 0)
                pageIndex = pageNumber.Value - 1;
            var pageSize = 10;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var list = await _backInStockSubscriptionService.GetAllSubscriptionsByCustomerIdAsync(customer.Id,
                (await _storeContext.GetCurrentStoreAsync()).Id, pageIndex, pageSize);

            var model = new CustomerBackInStockSubscriptionsModel();

            foreach (var subscription in list)
            {
                var product = await _productService.GetProductByIdAsync(subscription.ProductId);

                if (product != null)
                {
                    var subscriptionModel = new CustomerBackInStockSubscriptionsModel.BackInStockSubscriptionModel
                    {
                        Id = subscription.Id,
                        ProductId = product.Id,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(product)
                    };
                    model.Subscriptions.Add(subscriptionModel);
                }
            }

            model.PagerModel = new PagerModel
            {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerBackInStockSubscriptions",
                UseRouteLinks = true,
                RouteValues = new BackInStockSubscriptionsRouteValues { pageNumber = pageIndex }
            };

            return ApiResponseFactory.Success(model.ToDto<CustomerBackInStockSubscriptionsModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(CustomerBackInStockSubscriptionsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CustomerSubscriptionsPOST([FromBody] IDictionary<string, string> form)
        {
            foreach (var key in form.Keys)
            {
                var value = form[key];

                if (value.Equals("on") && key.StartsWith("biss", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("biss", string.Empty).Trim();
                    if (int.TryParse(id, out var subscriptionId))
                    {
                        var subscription =
                            await _backInStockSubscriptionService.GetSubscriptionByIdAsync(subscriptionId);
                        if (subscription != null &&
                            subscription.CustomerId == (await _workContext.GetCurrentCustomerAsync()).Id)
                            await _backInStockSubscriptionService.DeleteSubscriptionAsync(subscription);
                    }
                }
            }

            return await CustomerSubscriptions(0);
        }

        #endregion
    }
}
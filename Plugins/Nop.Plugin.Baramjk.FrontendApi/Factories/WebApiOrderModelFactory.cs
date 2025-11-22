using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Orders;
using Nop.Plugin.Baramjk.FrontendApi.Exceptions.Vendors;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Models.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class WebApiOrderModelFactory : IWebApiOrderModelFactory
    {
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        public WebApiOrderModelFactory(ILocalizationService localizationService, MediaSettings mediaSettings,
            IOrderModelFactory orderModelFactory, IOrderService orderService, IPictureService pictureService,
            IProductService productService, IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IWebHelper webHelper, IWorkContext workContext, IDateTimeHelper dateTimeHelper,
            IOrderProcessingService orderProcessingService, IPriceFormatter priceFormatter,
            ICurrencyService currencyService, IPictureModelFactory pictureModelFactory)
        {
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _orderModelFactory = orderModelFactory;
            _orderService = orderService;
            _pictureService = pictureService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
            _orderProcessingService = orderProcessingService;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
        }
        
        public async Task<ReOrderDto> PrepareReOrderDtoAsync(ReOrderServiceResults serviceResults)
        {
            return new ReOrderDto
            {
                Success = serviceResults.Success,
                FailedProducts = await PrepareReOrderProductDtosAsync(serviceResults.FailedProducts),
                AddedProducts = await PrepareReOrderProductDtosAsync(serviceResults.AddedProducts),
                Message = serviceResults.Success ? string.Empty : await _localizationService.GetResourceAsync("Baramjk.FrontendApi.Order.ReOrder.Message")
            };
        }

        private async Task<List<ReOrderProductDto>> PrepareReOrderProductDtosAsync(List<Product> products)
        {
            return await products.SelectAwait(async product => new ReOrderProductDto
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, p => p.Name),
                ImageUrl = await PrepareProductDefaultImagesAsync(product)
                
            }).ToListAsync();
        }
        
        private async Task<string> PrepareProductDefaultImagesAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //prepare picture models
            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopModelCacheDefaults.ProductDetailsPicturesModelKey
                , product, _mediaSettings.ProductDetailsPictureSize, false,
                await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync());

            var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                var defaultPicture = pictures.FirstOrDefault();
                var (fullSizeImageUrl, _) = await _pictureService.GetPictureUrlAsync(defaultPicture);
                return fullSizeImageUrl;
            });

            return cachedPictures;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeSumOrderTotal"></param>
        /// <returns></returns>
        /// <exception cref="VendorNotFoundException"></exception>
        public virtual async Task<CustomerOrderListModelDto> VendorOrdersSync(bool includeSumOrderTotal = false)
        {

            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var vendor = await _workContext.GetCurrentVendorAsync();
            if (vendor == null)
                throw new VendorNotFoundException();
            var vendorId = vendor.Id;
            var orders = await _orderService.SearchOrdersAsync(storeId, vendorId);

            var model = await PrepareCustomerOrderListModelAsync(orders);
            var modelDto = model.ToDto<CustomerOrderListModelDto>();

            if (includeSumOrderTotal)
            {
                var sum = orders.Sum(o => o.OrderTotal);
                modelDto.CustomProperties.Add("order_total_sum", sum);
            }

            return modelDto;
        }

        public virtual async Task<CustomerOrderListModelDto> CustomerOrdersSync(bool withFirstProductPicture = false,
            int? orderStatus = null)
        {
            var modelDto = await PrepareCustomerOrderListModelAsync();
            if (orderStatus != null)
                modelDto.Orders = modelDto.Orders.Where(i => i.OrderStatusEnum == orderStatus.Value).ToList();
            
            if (withFirstProductPicture == false)
                return modelDto;

            foreach (var order in modelDto.Orders)
            {
                var items = await _orderService.GetOrderItemsAsync(order.Id);
                var item = items.FirstOrDefault();
                if (item == null)
                    continue;

                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    continue;

                var pictureModel = await PrepareProductOverviewPictureModelAsync(product);
                order.FirstProductPicture = pictureModel;

                foreach (var orderItem in items)
                {
                    var itemProduct = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    var itemPrdouctPicture = await PrepareProductOverviewPictureModelAsync(itemProduct);
                    order.Items.Add(new OrderDetailsModelDto.OrderItemModelDto
                    {
                        OrderItemGuid = orderItem.OrderItemGuid,
                        Sku = itemProduct.Sku,
                        ProductId = orderItem.ProductId,
                        ProductName = itemProduct.Name,
                        UnitPrice = await _priceFormatter.FormatPriceAsync(orderItem.UnitPriceExclTax),
                        SubTotal = await _priceFormatter.FormatPriceAsync(itemProduct.Price),
                        Quantity = orderItem.Quantity,
                        AttributeInfo = orderItem.AttributeDescription,
                        Picture = itemPrdouctPicture,
                        ShortDescription = itemProduct.ShortDescription
                    });
                }
            }
            
            return modelDto;
        }


        public virtual async Task<OrderDetailsModelDto> PrepareOrderDetailsModelAsync(Order order)
        {
            var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
            var modelDto = model.ToFrameworkDto<OrderDetailsModelDto>();
            foreach (var item in modelDto.Items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    continue;

                item.ShortDescription = product.ShortDescription;
                item.FullDescription = product.FullDescription;
                item.Picture = await PrepareProductOverviewPictureModelAsync(product);
            }

            return modelDto;
        }


        protected virtual async Task<PictureModel> PrepareProductOverviewPictureModelAsync(Product product,
            int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopModelCacheDefaults.ProductDefaultPictureModelKey,
                product, pictureSize, true, await _workContext.GetWorkingLanguageAsync(),
                _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync());

            var defaultPictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                var pictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)
                        ? picture.TitleAttribute
                        : string.Format(
                            await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                            productName),
                    //"alt" attribute
                    AlternateText = picture != null && !string.IsNullOrEmpty(picture.AltAttribute)
                        ? picture.AltAttribute
                        : string.Format(
                            await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                            productName)
                };

                return pictureModel;
            });

            return defaultPictureModel;
        }
        
        protected virtual async Task<CustomerOrderListModelDto> PrepareCustomerOrderListModelAsync()
        {
            var model = new CustomerOrderListModelDto();
            var orders = await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id);
            foreach (var order in orders)
            {
                var orderModel = new CustomerOrderListModelDto.CustomerOrderDetailsModelDto
                {
                    Id = order.Id,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusEnum = (int) order.OrderStatus,
                    OrderStatusId = (int) order.OrderStatus,
                    PaymentStatusId = (int) order.OrderStatus,
                    ShippingStatusId = (int) order.ShippingStatus,
                    OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                    IsReturnRequestAllowed = await _orderProcessingService.IsReturnRequestAllowedAsync(order),
                    CustomOrderNumber = order.CustomOrderNumber
                };
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);

                model.Orders.Add(orderModel);
            }

            var recurringPayments = await _orderService.SearchRecurringPaymentsAsync((await _storeContext.GetCurrentStoreAsync()).Id,
                (await _workContext.GetCurrentCustomerAsync()).Id);
            foreach (var recurringPayment in recurringPayments)
            {
                var order = await _orderService.GetOrderByIdAsync(recurringPayment.InitialOrderId);

                var recurringPaymentModel = new RecurringOrderModelDto
                {
                    Id = recurringPayment.Id,
                    StartDate = (await _dateTimeHelper.ConvertToUserTimeAsync(recurringPayment.StartDateUtc, DateTimeKind.Utc)).ToString(),
                    CycleInfo = $"{recurringPayment.CycleLength} {await _localizationService.GetLocalizedEnumAsync(recurringPayment.CyclePeriod)}",
                    NextPayment = await _orderProcessingService.GetNextPaymentDateAsync(recurringPayment) is DateTime nextPaymentDate ? (await _dateTimeHelper.ConvertToUserTimeAsync(nextPaymentDate, DateTimeKind.Utc)).ToString() : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = await _orderProcessingService.GetCyclesRemainingAsync(recurringPayment),
                    InitialOrderId = order.Id,
                    InitialOrderNumber = order.CustomOrderNumber,
                    CanCancel = await _orderProcessingService.CanCancelRecurringPaymentAsync(await _workContext.GetCurrentCustomerAsync(), recurringPayment),
                    CanRetryLastPayment = await _orderProcessingService.CanRetryLastRecurringPaymentAsync(await _workContext.GetCurrentCustomerAsync(), recurringPayment)
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }

            return model;
        }

        private async Task<CustomerOrderListModel> PrepareCustomerOrderListModelAsync(IPagedList<Order> orders)
        {
            var model = new CustomerOrderListModel();

            foreach (var order in orders)
            {
                var orderModel = new CustomerOrderListModel.OrderDetailsModel
                {
                    Id = order.Id,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc),
                    OrderStatusEnum = order.OrderStatus,
                    OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
                    PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus),
                    ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
                    IsReturnRequestAllowed = await _orderProcessingService.IsReturnRequestAllowedAsync(order),
                    CustomOrderNumber = order.CustomOrderNumber
                };
                var orderTotalInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                orderModel.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true,
                    order.CustomerCurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);

                model.Orders.Add(orderModel);
            }

            var recurringPayments = await _orderService.SearchRecurringPaymentsAsync(
                (await _storeContext.GetCurrentStoreAsync()).Id,
                (await _workContext.GetCurrentCustomerAsync()).Id);
            foreach (var recurringPayment in recurringPayments)
            {
                var order = await _orderService.GetOrderByIdAsync(recurringPayment.InitialOrderId);

                var recurringPaymentModel = new CustomerOrderListModel.RecurringOrderModel
                {
                    Id = recurringPayment.Id,
                    StartDate = (await _dateTimeHelper.ConvertToUserTimeAsync(recurringPayment.StartDateUtc,
                        DateTimeKind.Utc)).ToString(),
                    CycleInfo =
                        $"{recurringPayment.CycleLength} {await _localizationService.GetLocalizedEnumAsync(recurringPayment.CyclePeriod)}",
                    NextPayment =
                        await _orderProcessingService.GetNextPaymentDateAsync(recurringPayment) is DateTime
                            nextPaymentDate
                            ? (await _dateTimeHelper.ConvertToUserTimeAsync(nextPaymentDate, DateTimeKind.Utc))
                            .ToString()
                            : "",
                    TotalCycles = recurringPayment.TotalCycles,
                    CyclesRemaining = await _orderProcessingService.GetCyclesRemainingAsync(recurringPayment),
                    InitialOrderId = order.Id,
                    InitialOrderNumber = order.CustomOrderNumber,
                    CanCancel = await _orderProcessingService.CanCancelRecurringPaymentAsync(
                        await _workContext.GetCurrentCustomerAsync(), recurringPayment),
                    CanRetryLastPayment =
                        await _orderProcessingService.CanRetryLastRecurringPaymentAsync(
                            await _workContext.GetCurrentCustomerAsync(), recurringPayment)
                };

                model.RecurringOrders.Add(recurringPaymentModel);
            }

            return model;
        }
    }
}
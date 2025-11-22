using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class DownloadController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public DownloadController(CustomerSettings customerSettings,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext)
        {
            _customerSettings = customerSettings;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _orderService = orderService;
            _productService = productService;
            _workContext = workContext;
        }

        #endregion

        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Methods

        /// <summary>
        ///     Sample
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Sample(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return ApiResponseFactory.NotFound($"The product by id={productId} is not found.");

            if (!product.HasSampleDownload)
                return ApiResponseFactory.NotFound("Product doesn't have a sample download.");

            var download = await _downloadService.GetDownloadByIdAsync(product.SampleDownloadId);
            if (download == null)
                return ApiResponseFactory.NotFound("Sample download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return Redirect(download.DownloadUrl);

            if (download.DownloadBinary == null)
                return ApiResponseFactory.NotFound("Download data is not available any more.");

            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : MimeTypes.ApplicationOctetStream;

            return new FileContentResult(download.DownloadBinary, contentType)
                { FileDownloadName = fileName + download.Extension };
        }

        /// <summary>
        ///     Get download
        /// </summary>
        /// <param name="orderItemGuid">Order item GUID</param>
        /// <param name="agree">Is agree</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> GetDownload([FromQuery] [Required] Guid orderItemGuid,
            [FromQuery] bool agree = false)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemGuid);
            if (orderItem == null)
                return ApiResponseFactory.NotFound($"The order item by GUID={orderItemGuid} is not found.");

            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

            if (!await _orderService.IsDownloadAllowedAsync(orderItem))
                return ApiResponseFactory.NotFound("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
                if (order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return ApiResponseFactory.BadRequest("This is not your order");

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            var download = await _downloadService.GetDownloadByIdAsync(product.DownloadId);
            if (download == null)
                return ApiResponseFactory.NotFound("Download is not available any more.");

            if (product.HasUserAgreement && !agree)
                return ApiResponseFactory.BadRequest("Not agree with the user agreement");

            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return ApiResponseFactory.BadRequest(string.Format(
                    await _localizationService.GetResourceAsync("DownloadableProducts.ReachedMaximumNumber"),
                    product.MaxNumberOfDownloads));

            if (download.UseDownloadUrl)
            {
                //increase download
                orderItem.DownloadCount++;
                await _orderService.UpdateOrderItemAsync(orderItem);

                //return result
                //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
                //In this case, it is not relevant. Url may not be local.
                return Redirect(download.DownloadUrl);
            }

            //binary download
            if (download.DownloadBinary == null)
                return ApiResponseFactory.NotFound("Download data is not available any more.");

            //increase download
            orderItem.DownloadCount++;
            await _orderService.UpdateOrderItemAsync(orderItem);

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : MimeTypes.ApplicationOctetStream;

            return new FileContentResult(download.DownloadBinary, contentType)
            {
                FileDownloadName = fileName + download.Extension
            };
        }

        /// <summary>
        ///     Get license
        /// </summary>
        /// <param name="orderItemGuid">Orer item GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> GetLicense([FromQuery] [Required] Guid orderItemGuid)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemGuid);
            if (orderItem == null)
                return ApiResponseFactory.NotFound($"The order item by GUID={orderItemGuid} is not found.");

            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

            if (!await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
                return ApiResponseFactory.NotFound("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
                if (await _workContext.GetCurrentCustomerAsync() == null ||
                    order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                    return ApiResponseFactory.BadRequest("This is not your order");

            var download = await _downloadService.GetDownloadByIdAsync(orderItem.LicenseDownloadId ?? 0);
            if (download == null)
                return ApiResponseFactory.NotFound("Download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return Redirect(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return ApiResponseFactory.NotFound("Download data is not available any more.");

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename)
                ? download.Filename
                : orderItem.ProductId.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : MimeTypes.ApplicationOctetStream;

            return new FileContentResult(download.DownloadBinary, contentType)
                { FileDownloadName = fileName + download.Extension };
        }

        /// <summary>
        ///     Get file upload
        /// </summary>
        /// <param name="downloadGuid">Download GUID</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> GetFileUpload([FromQuery] [Required] Guid downloadGuid)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
            if (download == null)
                return ApiResponseFactory.NotFound("Download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return Redirect(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return ApiResponseFactory.NotFound("Download data is not available any more.");

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadGuid.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : MimeTypes.ApplicationOctetStream;

            return new FileContentResult(download.DownloadBinary, contentType)
                { FileDownloadName = fileName + download.Extension };
        }

        /// <summary>
        ///     Get order note file
        /// </summary>
        /// <param name="orderNoteId">Order note identifier</param>
        [HttpGet("{orderNoteId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public virtual async Task<IActionResult> GetOrderNoteFile(int orderNoteId)
        {
            var orderNote = await _orderService.GetOrderNoteByIdAsync(orderNoteId);
            if (orderNote == null)
                return ApiResponseFactory.NotFound($"The order note by id={orderNoteId} is not found.");

            var order = await _orderService.GetOrderByIdAsync(orderNote.OrderId);

            if (await _workContext.GetCurrentCustomerAsync() == null ||
                order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return ApiResponseFactory.BadRequest("This is not your order");

            var download = await _downloadService.GetDownloadByIdAsync(orderNote.DownloadId);
            if (download == null)
                return ApiResponseFactory.NotFound("Download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return Redirect(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return ApiResponseFactory.NotFound("Download data is not available any more.");

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : orderNote.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : MimeTypes.ApplicationOctetStream;

            return new FileContentResult(download.DownloadBinary, contentType)
                { FileDownloadName = fileName + download.Extension };
        }

        #endregion
    }
}
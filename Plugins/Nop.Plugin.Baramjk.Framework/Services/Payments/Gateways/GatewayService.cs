using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Services.Networks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public class GatewayService : IGatewayService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITranslationService _translationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger _logger;

        public GatewayService(IHttpContextAccessor httpContextAccessor, ITranslationService translationService,
            IEventPublisher eventPublisher, ILogger logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _translationService = translationService;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }
        
        /// <exception cref="BadRequestBusinessException"></exception>
        public async Task<HttpResponse<CreateInvoiceResponse>> SendInvoiceAsync(IGatewayClient gatewayClient,
            GatewayPaymentTranslation translation, InvoiceRequest invoiceRequest)
        {
            if (string.IsNullOrEmpty(translation.Guid))
                throw new BadRequestBusinessException("Guild is invalid");

            invoiceRequest.SetClientReferenceId(translation.Guid);

            SetCallBackUrl(gatewayClient, invoiceRequest);

            var invoiceResponse = await SendAsync(gatewayClient, invoiceRequest);
            await UpdateTransactionAsync(translation, gatewayClient.GatewayName, invoiceResponse);

            var invoiceEvent =
                new GatewayPaymentTranslationCreateInvoiceEvent(translation, invoiceResponse.IsSuccess());
            await _eventPublisher.PublishAsync(invoiceEvent);

            return invoiceResponse;
        }

        private async Task UpdateTransactionAsync(GatewayPaymentTranslation translation, string gatewayName,
            HttpResponse<CreateInvoiceResponse> invoiceHttpResponse)
        {
            translation.GatewayName = gatewayName;
            translation.InvoiceId = invoiceHttpResponse?.Body?.InvoiceId;
            translation.PaymentUrl = invoiceHttpResponse?.Body?.PaymentUrl;

            await _translationService.UpdateAsync(translation);
        }
        
        /// <exception cref="BadRequestBusinessException"></exception>
        private async Task<HttpResponse<CreateInvoiceResponse>> SendAsync(IGatewayClient gatewayClient,
            InvoiceRequest invoiceRequest)
        {
            var invoiceResponse = await gatewayClient.CreateInvoiceAsync(invoiceRequest);
            if (invoiceResponse?.Body == null)
            {
                var msg =
                    $"Create invoice was not success. Data is null, Request: {invoiceRequest.ToJson()}, Response:" +
                    (invoiceResponse == null ? "" : invoiceResponse.ToJson());
                await _logger.ErrorAsync(msg);
                throw new BadRequestBusinessException("Create invoice was not success. Data is null");
            }

            if (invoiceResponse.IsSuccess() == false)
            {
                var msg = invoiceResponse.GetMessage("Create invoice was not success.");
                msg += invoiceRequest.ToJson();
                await _logger.ErrorAsync(msg);
            }

            var paymentUrl = invoiceResponse.Body.PaymentUrl;
            if (string.IsNullOrEmpty(paymentUrl))
            {
                await _logger.ErrorAsync($"PaymentUrl is invalid - {invoiceResponse.GetMessage("No meesage")} - Response : {invoiceResponse.ToJson()}");
                throw new BadRequestBusinessException("PaymentUrl is invalid");
            }
                

            return invoiceResponse;
        }

        private void SetCallBackUrl(IGatewayClient gatewayClient, InvoiceRequest request)
        {
            if (_httpContextAccessor.HttpContext == null)
                return;

            var host = _httpContextAccessor.HttpContext.Request.Host.ToString();
            if (host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                host = "http://redirect.k-pack.online";

            if (host.StartsWith("http") == false)
                host = "http://" + host;

            var errorUrl = $"{host}{gatewayClient.ErrorCallBackUrl}";
            var successCallBackUrl = $"{host}{gatewayClient.SuccessCallBackUrl}";

            request.SetCallBackUrl(successCallBackUrl, errorUrl);
        }
    }
}
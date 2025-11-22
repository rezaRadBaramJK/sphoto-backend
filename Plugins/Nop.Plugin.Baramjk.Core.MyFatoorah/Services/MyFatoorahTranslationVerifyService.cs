using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Services
{
    public class MyFatoorahTranslationVerifyService : TranslationVerifyService,
        IMyFatoorahTranslationVerifyPaymentService
    {
        private static readonly Type _clientType = typeof(IMyFatoorahPaymentClient);

        private readonly IGatewayClientProvider _gatewayClientProvider;

        public MyFatoorahTranslationVerifyService(ITranslationService translationService,
            IGatewayClientProvider gatewayClientProvider, IEventPublisher eventPublisher)
            : base(translationService, eventPublisher)
        {
            _gatewayClientProvider = gatewayClientProvider;
        }

        [HttpPost]
        public async Task<VerifyResult> VerifyByPaymentIdAsync(string paymentId)
        {
            var gatewayClient = _gatewayClientProvider.GetGatewayClient(_clientType);
            var statusRequest = GetPaymentStatusRequest.ByPaymentId(paymentId);
            return await VerifyAsync(gatewayClient, statusRequest);
        }

        [HttpPost]
        public async Task<VerifyResult> VerifyByInvoiceIdAsync(int invoiceId)
        {
            var gatewayClient = _gatewayClientProvider.GetGatewayClient(_clientType);
            var statusRequest = GetPaymentStatusRequest.ByInvoiceId(invoiceId.ToString());
            return await VerifyAsync(gatewayClient, statusRequest);
        }

        public Task<VerifyResult> VerifyAsync()
        {
            throw new NotImplementedException();
        }
    }
}
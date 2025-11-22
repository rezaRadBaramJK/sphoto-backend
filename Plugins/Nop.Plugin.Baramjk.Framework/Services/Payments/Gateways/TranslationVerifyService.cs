using System.Threading.Tasks;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Networks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public class TranslationVerifyService : ITranslationVerifyService
    {
        private readonly ITranslationService _translationService;
        private readonly IEventPublisher _eventPublisher;

        public TranslationVerifyService(ITranslationService translationService, IEventPublisher eventPublisher)
        {
            _translationService = translationService;
            _eventPublisher = eventPublisher;
        }

        public async Task<VerifyResult> VerifyAsync(IGatewayClient gatewayClient,
            GetInvoiceStatusRequest request)
        {
            var response = await gatewayClient.GetInvoiceStatusAsync(request);
            if (response.IsSuccess() == false)
            {
                if (response?.Body == null)
                    return new VerifyResult()
                    {
                        Status = VerifyStatus.Unknown,
                        Message = response.GetMessage("Verify invoice was not success")
                    };

                return new VerifyResult()
                {
                    Status = VerifyStatus.Fail,
                    Message = response.GetMessage("Verify invoice was not success")
                };
            }

            var invoiceStatusResponse = response.Body;
            var guid = invoiceStatusResponse.ClientReferenceId;
            var translation = await _translationService.GetByGuidAsync(guid);
            if (translation == null)
            {
                return new VerifyResult()
                {
                    Status = VerifyStatus.InvalidTranslation,
                    Message = "Translation not found"
                };
            }

            if (translation.Status == GatewayPaymentStatus.Paid)
            {
                return new VerifyResult
                {
                    Status = VerifyStatus.AlreadyPaid,
                    Message = "Translation is already paid",
                    Translation = translation,
                    IsPaid = true
                };
            }

            var oldTranslationStatus = translation.Status;
            var paymentStatus = invoiceStatusResponse.PaymentStatus;
            var verifyStatus = VerifyStatus.SuccessProcess;
            if (invoiceStatusResponse.Amount < translation.AmountToPay)
            {
                paymentStatus = GatewayPaymentStatus.Invalid;
                verifyStatus = VerifyStatus.Invalid;
            }

            translation.Status = paymentStatus;
            translation.AmountPayed = invoiceStatusResponse.Amount;
            translation.InvoiceId = invoiceStatusResponse.InvoiceId;
            await _translationService.UpdateAsync(translation);

            var statusEvent = new GatewayPaymentTranslationStatusEvent(translation, oldTranslationStatus);
            await _eventPublisher.PublishAsync(statusEvent);

            if (!statusEvent.IsOwnerConsumerHandled)
            {
                if (verifyStatus != VerifyStatus.SuccessProcess)
                    verifyStatus = VerifyStatus.NotHandledByOwner;

                return new VerifyResult
                {
                    Status = verifyStatus,
                    Message = response.GetMessage("Success"),
                    Translation = translation,
                    IsPaid = invoiceStatusResponse.IsPaid,
                    ConsumerCallBackUrl = string.Format(translation.ConsumerCallBackUrl, translation.Guid),
                };
            }

            var result = new VerifyResult
            {
                Status = verifyStatus,
                Message = response.GetMessage("Success"),
                Translation = translation,
                ConsumerCallBackUrl = string.Format(translation.ConsumerCallBackUrl, translation.Guid),
                IsPaid = invoiceStatusResponse.IsPaid,
                ConsumerResult = statusEvent.ConsumerResult
            };
            return result;
        }
    }
}
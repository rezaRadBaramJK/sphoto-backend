using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public static class Extensions
    {
        public static bool NeedToProcess(this GatewayPaymentTranslation translation)
        {
            return translation != null &&
                   (translation.Status == GatewayPaymentStatus.Paid) &&
                   (translation.ConsumerStatus != ConsumerStatus.Success);
        }

        public static GatewayPaymentTranslationResponse ToResponseModel(this GatewayPaymentTranslation translation)
        {
            return new GatewayPaymentTranslationResponse
            {
                Guid = translation.Guid,
                AmountPayed = translation.AmountToPay,
            };
        }

        public static VerifyTranslationResponseDto ToResponseModel(this VerifyResult model)
        {
            if (model == null)
                throw new NullBusinessException(nameof(model));

            var consumerStatus = model?.ConsumerResult?.ConsumerStatus ??
                                 (model?.Translation?.ConsumerStatus ?? ConsumerStatus.Failed);

            return new VerifyTranslationResponseDto
            {
                Message = model.Message,
                IsPaid = model.IsPaid,
                IsNewSuccessPaid = model.IsNewSuccessPaid,
                VerifyStatus = model.Status,
                Guid = model.Translation?.Guid,
                GatewayName = model.Translation?.GatewayName,
                ConsumerStatus = consumerStatus,
                TranslationPaymentStatus = model.Translation?.Status ?? GatewayPaymentStatus.Unknown,
                IsSuccessPaidProcess = model.IsPaid && model?.ConsumerResult?.ConsumerStatus == ConsumerStatus.Success,
            };
        }
    }
}
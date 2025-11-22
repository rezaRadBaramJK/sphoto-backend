using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public class VerifyResult
    {
        public GatewayPaymentTranslation Translation { get; set; }
        public VerifyStatus Status { get; set; }
        public string Message { get; set; }
        public bool IsPaid { get; set; }
        public string ConsumerCallBackUrl { get; set; }
        public bool IsNewSuccessPaid => IsPaid && Status == VerifyStatus.SuccessProcess;
        public ConsumerResult ConsumerResult { get; set; }
    }

    public enum VerifyStatus
    {
        Unknown,
        Fail,
        InvalidTranslation,
        Invalid,
        AlreadyPaid,
        NotHandledByOwner,
        SuccessProcess
    }
}
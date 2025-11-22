namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public class VerifyTranslationResponseDto
    {
        public string Message { get; set; }
        public bool IsPaid { get; set; }
        public bool IsNewSuccessPaid { get; set; }
        public string Guid { get; set; }
        public string GatewayName { get; set; }
        public bool IsSuccessPaidProcess { get; set; }
        public VerifyStatus VerifyStatus { get; set; }
        public ConsumerStatus ConsumerStatus { get; set; }
        public GatewayPaymentStatus TranslationPaymentStatus { get; set; }
    }
}
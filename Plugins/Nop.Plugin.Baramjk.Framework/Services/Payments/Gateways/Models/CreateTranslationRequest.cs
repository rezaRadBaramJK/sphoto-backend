namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models
{
    public class CreateTranslationRequest
    {
        public decimal AmountToPay { get; set; }
        public int CustomerId { get; set; }
        
        public string GatewayName { get; set; }

        public int PaymentOptionId { get; set; } = 0;

        public int PaymentFeeRuleId { get; set; } = 0;
        public decimal PaymentFeeValue { get; set; } = 0;
        
        public string ConsumerName { get; set; }
        public string ConsumerEntityType { get; set; }
        public int ConsumerEntityId { get; set; }
        public string ConsumerData { get; set; }
        /// <summary>
        ///     /YourPlugin/CallBack?guid={0}
        /// </summary>
        public string ConsumerCallBack { get; set; }
    }
}
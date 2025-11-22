namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins
{
    public static class DefaultValue
    {
        public const string CALL_BACK_URL = "/MyFatoorah/Payment/SuccessCallBack";
        public const string ERROR_URL = "/MyFatoorah/Payment/ErrorCallBack";
        public const string TABLENAME_PREFIX = "Core_MyFatoorah_";
        public const string PAYMENT_METHOD_SYSTEM_NAME = "Payments.MyFatoorah";
        public const string PAYMENT_METHOD_ID_ATTRIBUTE_NAME = "PaymentMethodId";
        public const string WebHookPaymentStatusChangedName = "PAYMENT_STATUS_CHANGED";
        public const int WebHookPaymentStatusChangedCode = 1;
        public const string WebHookPaidStatus = "PAID";
        public const string WebHookSignatureHeaderName = "myfatoorah-signature";
    }
}
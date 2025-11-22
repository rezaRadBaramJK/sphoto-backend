using System;
using System.Security.Cryptography;
using System.Text;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.WebHooks;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Helpers
{
    public static class SignatureHelper
    {
        
        /// <summary>
        /// Verify My Fatoorah signature base on documents: https://docs.myfatoorah.com/docs/webhook-signature
        /// </summary>
        /// <param name="apiParams"></param>
        /// <param name="requestSignature">Generated signature from My Fatoorah, It came from 'myfatoorah-signature' request header.</param>
        /// <returns></returns>
        public static VerifyWebHookSignatureResult VerifyPaymentStatus(WebHookApiParams apiParams, string requestSignature)
        {
            //validations
            if (string.IsNullOrEmpty(requestSignature))
                return VerifyWebHookSignatureResult.GetFailedResult("Invalid signature.");
            
            var myFatoorahSettings = EngineContext.Current.Resolve<MyFatoorahSettings>();
            
            if(string.IsNullOrEmpty(myFatoorahSettings.WebhookSecretKey))
                return VerifyWebHookSignatureResult.GetFailedResult("Webhook secret key not set.");
            //create the raw signature base on my fatoorah docs order: https://docs.myfatoorah.com/docs/webhook-v2-payment-status-data-model
            var invoice = apiParams.Data.Invoice;
            var transaction = apiParams.Data.Transaction;

            var rawSignature =  new StringBuilder()
                .Append($"Invoice.Id={invoice.Id},")
                .Append($"Invoice.Status={invoice.Status},")
                .Append($"Transaction.Status={transaction.Status},")
                .Append($"Transaction.PaymentId={transaction.PaymentId},")
                .Append($"Invoice.ExternalIdentifier={invoice.ExternalIdentifier},")
                .ToString();
            
            var rowSignatureBytes = Encoding.UTF8.GetBytes(rawSignature);
            var encodedSecretKey = Encoding.UTF8.GetBytes(myFatoorahSettings.WebhookSecretKey);
            //generate signature
            using var hmac = new HMACSHA256(encodedSecretKey);
            var hashedSignature = hmac.ComputeHash(rowSignatureBytes);
            var expectedSignature = Convert.ToBase64String(hashedSignature);
            
            //verify
            return expectedSignature == requestSignature ? 
                VerifyWebHookSignatureResult.GetSuccessResult() :
                VerifyWebHookSignatureResult.GetFailedResult("Invalid signature.");
            
        }

       
        
    }
}
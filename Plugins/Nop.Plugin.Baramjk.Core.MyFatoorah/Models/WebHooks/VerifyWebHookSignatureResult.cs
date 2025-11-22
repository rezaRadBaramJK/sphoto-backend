namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.WebHooks
{
    public class VerifyWebHookSignatureResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public static VerifyWebHookSignatureResult GetSuccessResult() => new()
        {
            Success = true,
        };

        public static VerifyWebHookSignatureResult GetFailedResult(string errorMessage) => new()
        {

        };
    }
}
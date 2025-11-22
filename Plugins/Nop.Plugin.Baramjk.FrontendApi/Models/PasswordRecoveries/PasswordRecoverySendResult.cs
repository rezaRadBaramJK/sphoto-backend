using Nop.Web.Models.Customer;

namespace Nop.Plugin.Baramjk.FrontendApi.Models.PasswordRecoveries
{
    public class PasswordRecoverySendResult
    {
        public bool Success { get; set; }
        
        public PasswordRecoveryModel PasswordRecoveryModel { get; set; }
    }
}
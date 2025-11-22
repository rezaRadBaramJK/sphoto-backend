namespace Nop.Plugin.Baramjk.FrontendApi.Models
{
    public class AuthenticateCustomerRequest : Framework.Models.AuthenticateRequest
    {
        public bool IsGuest { get; set; }
    }
}
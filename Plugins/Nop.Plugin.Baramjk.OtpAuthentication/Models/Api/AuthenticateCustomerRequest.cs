using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Api
{
    public class AuthenticateCustomerRequest : AuthenticateRequest
    {
        public bool IsGuest { get; set; }
    }
}
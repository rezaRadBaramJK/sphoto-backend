
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Api
{
    
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SendOtpResponseApiModel
    {
        public bool Success { get; set; }
        [JsonIgnore] public List<string> ErrorMessages { get; set; } = new List<string>();
        public string Otp { get; set; }
    }
}
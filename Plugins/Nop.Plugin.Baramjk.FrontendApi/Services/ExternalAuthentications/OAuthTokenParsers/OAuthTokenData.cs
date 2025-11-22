using System;
using System.Collections.Generic;
using Nop.Services.Authentication.External;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.OAuthTokenParsers
{
    public class OAuthTokenData
    {
        public string ProviderName { get; set; }
        public string AccessToken { get; set; }
        public string Email { get; set; }
        public string ExternalIdentifier { get; set; }
        public string ExternalDisplayIdentifier { get; set; }
        public List<ExternalAuthenticationClaim> Claims { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Gender { get; set; }
        public string MobileNumber { get; set; }
        public string Avatar { get; set; }
        public string IdToken { get; set; }
    }
}
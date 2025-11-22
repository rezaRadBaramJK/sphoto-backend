using System;
using System.Security.Claims;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.Validators
{
    public class ValidateResult
    {
        public bool IsSuccess { get; set; }
        public ClaimsPrincipal Principal { get; set; }
        public Exception Exception { get; set; }
    }
}
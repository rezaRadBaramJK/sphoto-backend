using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings
{
    public class OtpAuthenticationSettingsModel
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public int TokenLifetimeSeconds { get; set; }
        public int RefreshTokenLifetimeSeconds { get; set; }
        public bool AllowRegisterationAsVendor { get; set; }
        public bool DontRegisterIfUserNotExists { get; set; }

        public bool ShowOtpCodeInResponse { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.OtpAuthentication.Admin.Configuration.SendMethod")]
        public int SendMethodId { get; set; }

        public IList<SelectListItem> AvailableMethods { get; set; } = new List<SelectListItem>();
    }
}
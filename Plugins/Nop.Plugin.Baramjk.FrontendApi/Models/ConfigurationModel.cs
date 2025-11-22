using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.FrontendApi.Models.Types;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.FrontendApi.Models
{
    /// <summary>
    ///     Represents plugin configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.WebApi.Frontend.DeveloperMode")]
        public bool DeveloperMode { get; set; }

        [NopResourceDisplayName("Plugins.WebApi.Frontend.SecretKey")]
        public string SecretKey { get; set; }

        public int TokenLifetimeDays { get; set; }
        public int TokenLifetimeMinutes { get; set; }

        public string GoogleIOSClientId { get; set; }
        public string AppleIOSClientId { get; set; }
        public string FaceBookIOSClientId { get; set; }

        public string GoogleAndroidClientId { get; set; }
        public string AppleAndroidClientId { get; set; }
        public string FaceBookAndroidClientId { get; set; }

        public string GoogleWebClientId { get; set; }
        public string AppleWebClientId { get; set; }
        public string FaceBookWebClientId { get; set; }
        public bool DontRequireAddress { get; set; }
        
        [NopResourceDisplayName("Baramjk.FrontendApi.Settings.UploadFileSupportedTypes")]
        public string UploadFileSupportedTypes { get; set; } = string.Empty;
        
        [NopResourceDisplayName("Baramjk.FrontendApi.Settings.UploadFileMaxSize")]
        public double UploadFileMaxSize { get; set; } = 2;
        
        [NopResourceDisplayName("Baramjk.FrontendApi.Settings.CustomFrontendBaseUrl")]
        public string CustomFrontendBaseUrl { get; set; }
        
        [NopResourceDisplayName("Baramjk.FrontendApi.Settings.PasswordRecoveryStrategy")]
        public int PasswordRecoveryStrategy { get; set; }
        
        public IList<SelectListItem> PasswordRecoveryStrategies { get; set; }
        
    }
}
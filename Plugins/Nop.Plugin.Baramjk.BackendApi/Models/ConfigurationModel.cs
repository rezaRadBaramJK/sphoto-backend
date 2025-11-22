using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.BackendApi.Models
{
    /// <summary>
    ///     Represents plugin configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.WebApi.Backend.DeveloperMode")]
        public bool DeveloperMode { get; set; }

        [NopResourceDisplayName("Plugins.WebApi.Backend.SecretKey")]
        public string SecretKey { get; set; }
        [NopResourceDisplayName("Plugins.WebApi.Backend.CorsOrigins")]

        public string CorsOrigins { get; set; }

    }
}
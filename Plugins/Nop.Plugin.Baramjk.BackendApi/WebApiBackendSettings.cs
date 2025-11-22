using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.BackendApi
{
    /// <summary>
    ///     Represents settings of the Web API Backend
    /// </summary>
    public class WebApiBackendSettings : ISettings
    {
        public bool DeveloperMode { get; set; }
        public string SecretKey { get; set; }
        public int TokenLifetimeDays { get; set; }
        public string CorsOrigins { get; set; }
    }
}
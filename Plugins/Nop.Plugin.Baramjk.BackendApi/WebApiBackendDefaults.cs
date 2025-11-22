namespace Nop.Plugin.Baramjk.BackendApi
{
    /// <summary>
    ///     Represents plugin constants
    /// </summary>
    public class WebApiBackendDefaults
    {
        /// <summary>
        ///     Gets swagger document version
        /// </summary>
        public const string API_VERSION = "v1.06";
        
        public const string Payment_CashOnDelivery_SysName = "Payments.CashOnDelivery";
        /// <summary>
        ///     Gets a plugin system name
        /// </summary>
        public static string SystemName => "Baramjk.BackendApi";

        /// <summary>
        ///     Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Baramjk.BackendApi.Configure";
    }
}
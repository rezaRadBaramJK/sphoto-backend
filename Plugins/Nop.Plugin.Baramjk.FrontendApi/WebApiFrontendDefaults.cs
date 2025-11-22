using Nop.Core.Caching;

namespace Nop.Plugin.Baramjk.FrontendApi
{
    public class WebApiFrontendDefaults
    {
        public const string API_VERSION = "v1.0";
        public const string SYSTEM_NAME = "Baramjk.FrontendApi";
        public const string CONFIGURATION_ROUTE_NAME = "Baramjk.FrontendApi.Configure";
        public const string ROUTE = "FrontendApi/[controller]/[action]";
        public const string ROUTE2 = "FrontendApi/";
        public const string AREA = "FrontendApi";
        public static CacheKey PaymentInfoKeyCache = new("Baramjk.FrontendApi.PaymentInfo-{0}-{1}");

        public const string CustomerGenerateNewPasswordRecoveryMessage = "Customer.GenerateNewPasswordRecovery";
    }
}
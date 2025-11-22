using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.Framework
{
    public class FrameworkSettings : ISettings
    {
        public string DefaultGatewayClientName { get; set; } = "MyFatoorahPaymentClient";  
    }
}
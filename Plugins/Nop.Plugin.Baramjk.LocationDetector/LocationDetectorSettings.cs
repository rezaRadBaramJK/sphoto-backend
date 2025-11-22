using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.LocationDetector
{
    public class LocationDetectorSettings : ISettings
    {
        public string DefaultCurrency { get; set; }
        public string Ip2LocationToken { get; set; }
        public bool SwitchCustomerCurrencyOnChangingCountry { get; set; }
    }
}
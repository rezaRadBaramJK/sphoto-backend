using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.LocationDetector.Models
{
    public record LocationDetectorSettingsModel : BaseNopModel
    {
        public string DefaultCurrency { get; set; }
        public string Ip2LocationToken { get; set; }
        public bool SwitchCustomerCurrencyOnChangingCountry { get; set; }
    }
}
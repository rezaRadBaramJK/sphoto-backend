using Nop.Core;

namespace Nop.Plugin.Baramjk.LocationDetector.Domain
{
    public class CountryCurrencyMapping : BaseEntity
    {
        public int Id { get; set; }
        public string IsoCountryCode { get; set; }
        public string IsoCurrencyCode { get; set; }
    }
}
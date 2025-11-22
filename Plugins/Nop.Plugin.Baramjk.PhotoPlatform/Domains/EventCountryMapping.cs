using Nop.Core;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class EventCountryMapping: BaseEntity
    {
        public int EventId { get; set; }
        public int CountryId { get; set; }
        
    }
}
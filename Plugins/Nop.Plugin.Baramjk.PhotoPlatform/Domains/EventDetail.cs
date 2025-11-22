using System;
using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class EventDetail : BaseEntity, ILocalizedEntity
    {
        public string TermsAndConditions { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int TimeSlotDuration { get; set; }

        public int EventId { get; set; }

        public decimal PhotoPrice { get; set; }

        public decimal ProductionShare { get; set; }

        public decimal ActorShare { get; set; }

        public decimal PhotoShootShare { get; set; }
        
        public string Note { get; set; }
        
        public string LocationUrlTitle { get; set; }
        
        public string LocationUrl { get; set; }
    }
}
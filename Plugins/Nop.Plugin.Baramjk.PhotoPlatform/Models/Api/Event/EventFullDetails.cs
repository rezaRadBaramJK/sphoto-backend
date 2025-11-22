using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event
{
    public class EventFullDetails
    {
        public Product Product { get; set; }

        public EventDetail EventDetail { get; set; }

        public CashierEvent CashierEvent { get; set; }

        public IEnumerable<TimeSlot> TimeSlots { get; set; }

        public IEnumerable<EventDetailActor> Actors { get; set; }
    }
}
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ProductionEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.SupervisorEvents;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events
{
    public record ProductDetailsWidgetViewModel : BaseNopModel
    {
        public EventDetailsViewModel EventDetails { get; set; }

        public EventTimeSlotsViewModel TimeSlots { get; set; }

        public EventActorEventsViewModel ActorEvents { get; set; }

        public EventCashierEventsViewModel CashierEvents { get; set; }

        public EventSupervisorEventsViewModel SupervisorEvents { get; set; }

        public EventProductionEventsViewModel ProductionEvents { get; set; }

        public CashierDailyBalanceSearchModel EventCashiersDailyBalances { get; set; }
    }
}
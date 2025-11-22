using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.SupervisorEvents
{
    public record EventSupervisorEventsViewModel : BaseSearchModel
    {
        public int EventId { get; set; }
    }
}
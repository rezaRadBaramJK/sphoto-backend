using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events
{
    public record EventCashierEventsViewModel : BaseSearchModel
    {
        public int EventId { get; set; }
    }
}
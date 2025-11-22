using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ProductionEvents
{
    public record EventProductionEventsViewModel : BaseSearchModel
    {
        public int EventId { get; set; }
    }
}
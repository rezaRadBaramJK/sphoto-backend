using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.TimeSlots
{
    public record TimeSlotItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Date")]
        public string Date { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.StartTime")]
        public string StartTime { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.EndTime")]
        public string EndTime { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Note")]
        public string Note { get; set; }
    }
}
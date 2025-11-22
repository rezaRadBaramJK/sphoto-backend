using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.TimeSlots
{
    public record TimeSlotViewModel : BaseNopEntityModel, ILocalizedModel<TimeSlotLocalizedModel>
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Date")]
        [UIHint("Date")]
        public DateTime Date { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.StartTime")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.EndTime")]
        [DataType(DataType.Time)]
        public TimeSpan? EndTime { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Note")]
        public string Note { get; set; }

        /// <summary>
        /// This is product id
        /// </summary>
        public int EventId { get; set; }

        public IList<TimeSlotLocalizedModel> Locales { get; set; } = new List<TimeSlotLocalizedModel>();
    }
}
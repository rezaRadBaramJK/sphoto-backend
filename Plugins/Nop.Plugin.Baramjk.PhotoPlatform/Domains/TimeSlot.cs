using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class TimeSlot : BaseEntity, ISoftDeletedEntity, ILocalizedEntity
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool Active { get; set; }
        public int EventId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string Note { get; set; }
        public bool Deleted { get; set; }
    }
}
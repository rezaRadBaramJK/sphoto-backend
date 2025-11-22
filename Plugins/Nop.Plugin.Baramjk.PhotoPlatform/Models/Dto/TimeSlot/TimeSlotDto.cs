using System;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.TimeSlot
{
    public class TimeSlotDto : CamelCaseModelWithIdDto
    {
        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public bool Active { get; set; }

        public int EventId { get; set; }
        
        public string Note { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.TimeSlot
{
    public class GroupedTimeSlotsDto : CamelCaseBaseDto
    {
        public string Label { get; set; }
        public DateTime Date { get; set; }
        public List<TimeSlotDto> TimeSlots { get; set; }
    }
}
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.TimeSlot;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reports
{
    public class ReportFiltersBaseDto: CamelCaseBaseDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public List<GroupedTimeSlotsDto> GroupedTimeSlots { get; set; }
    }
}
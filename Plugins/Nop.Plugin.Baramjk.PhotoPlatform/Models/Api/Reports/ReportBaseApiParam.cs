using System;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports
{
    public class ReportBaseApiParam
    {
        public DateTime? Date { get; set; }
        public int EventId { get; set; } = 0;
        public int? TimeSlotId { get; set; }
    }
}
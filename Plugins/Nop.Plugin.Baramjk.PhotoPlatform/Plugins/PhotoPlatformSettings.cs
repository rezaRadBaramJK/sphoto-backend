using Nop.Core.Configuration;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Types;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Plugins
{
    public class PhotoPlatformSettings : ISettings
    {
        public int TimeSlotLabelTypeId { get; set; } = (int)TimeSlotLabelType.Brief;
        public bool ShowTicketsInProductionReports { get; set; }

        public string OrderDetailsFrontendBaseUrl { get; set; }

        public string TicketSmsMessage { get; set; }
        
        public int BirthDayRewardPoints { get; set; }
    }
}
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reports
{
    public class RevenueReportOverviewBaseDto: CamelCaseBaseDto
    {
        
        public int CameraManPhotoCount { get; set; }
        public int CustomerMobilePhotoCount { get; set; }
        public string ActorShare { get; set; }
        public int TotalTickets { get; set; }
        public string TotalRevenue { get; set; }
        
    }
}
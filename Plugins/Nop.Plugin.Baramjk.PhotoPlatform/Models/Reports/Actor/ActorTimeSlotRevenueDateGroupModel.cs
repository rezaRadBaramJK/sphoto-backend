using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Actor
{
    public class ActorTimeSlotRevenueDateGroupModel
    {
        public string Date { get; set; }
        public int TotalDayPhotoCount { get; set; }
        public decimal TotalDayPhotoPrice { get; set; }
        public List<ActorTimeSlotRevenueItemModel> Items { get; set; } = new();
        
        
    }
}
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Actor
{
    public class ActorTimeSlotRevenueListModel
    {
        public List<ActorTimeSlotRevenueDateGroupModel> GroupedByDate { get; set; } = new();
        public int TotalPhotoCount { get; set; }
        public decimal TotalPhotoPrice { get; set; }
    }
}
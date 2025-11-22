namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Actor
{
    public class ActorTimeSlotRevenueItemModel
    {
        
        public string EventName { get; set; }
        public string TimeSlot { get; set; }
        public string Date { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal ActorShare { get; set; }
        public int CameraManPhotoCount { get; set; }
        public int CustomerMobilePhotoCount { get; set; }
        public int TotalPhotoCount { get; set; }
        public decimal TotalPhotoPrice { get; set; }
    }
}
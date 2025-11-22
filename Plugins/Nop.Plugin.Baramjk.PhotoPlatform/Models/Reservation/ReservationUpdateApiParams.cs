namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation
{
    public class ReservationUpdateApiParams
    {
        public int ReservationItemId { get; set; }
        
        public int CameraManPhotoCount { get; set; }

        public int CustomerMobilePhotoCount { get; set; }

        public int ReservationStatusId { get; set; }
    }
}
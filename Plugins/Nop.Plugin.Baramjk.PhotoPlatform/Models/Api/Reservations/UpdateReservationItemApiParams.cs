namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reservations
{
    public class UpdateReservationItemApiParams
    {
        public int ReservationId { get; set; }
        public int UsedCameraManPhotoCount { get; set; }
        public int UsedCustomerMobilePhotoCount { get; set; }
    }
}
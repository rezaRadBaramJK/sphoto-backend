namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reservations
{
    public class ChangePhotographyDetailsApiParams
    {
        public int ReservationId { get; set; }
        public bool SwitchCameraManPhotoCount { get; set; }
        public bool SwitchCustomerMobilePhotoCount { get; set; }
        public int NewActorId { get; set; }
    }
}
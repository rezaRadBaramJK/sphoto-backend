using Nop.Core;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class ReservationItem : BaseEntity
    {
        public int OrderId { get; set; }
        public int EventId { get; set; }
        public int ActorId { get; set; }
        public int TimeSlotId { get; set; }
        public int CameraManPhotoCount { get; set; }
        public int CustomerMobilePhotoCount { get; set; }
        public int ReservationStatusId { get; set; }
        
        public ReservationStatus ReservationStatus
        {
            get => (ReservationStatus)ReservationStatusId;
            set => ReservationStatusId = (int)value;
        }

        public int Queue { get; set; }
        
        public int UsedCameraManPhotoCount { get; set; }
        
        public int UsedCustomerMobilePhotoCount { get; set; }
    }
}
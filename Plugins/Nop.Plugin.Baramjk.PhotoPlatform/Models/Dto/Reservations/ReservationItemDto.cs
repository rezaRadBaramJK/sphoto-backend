using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Picture;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reservations
{
    public class ReservationItemDto : CamelCaseModelDto
    {
        public int EventId { get; set; }
        public int TimeSlotId { get; set; }
        public int Queue { get; set; }
        public string EventName { get; set; }
        public PictureDto Picture { get; set; }
        public string ReservationDate { get; set; }
        public string ReservationTime { get; set; }
        public int ReservationStatusId { get; set; }

        public ReservationStatusType ReservationStatus => (ReservationStatusType)ReservationStatusId;
        
        public bool IsEditable { get; set; }

        public List<PhotographyDetails> PhotographyDetails { get; set; }
    }
}
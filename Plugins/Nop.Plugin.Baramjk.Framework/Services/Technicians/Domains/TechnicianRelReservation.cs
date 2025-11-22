using System;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains
{
    public class TechnicianRelReservation : BaseEntity
    {
        public int TechnicianId { get; set; }
        public int ReservationId { get; set; }
        public int ReservationProcessId { get; set; }
        public TechnicianReservationProcessType ReservationProcess => (TechnicianReservationProcessType)ReservationProcessId;
        public int ReservationResultId { get; set; }
        public TechnicianReservationResultType ReservationResult => (TechnicianReservationResultType)ReservationResultId;
        public bool IsCompleted { get; set; }
        
        public DateTime CreatedOn { get; set; }
        
        public string Note { get; set; }
        public string PictureIds { get; set; }


    }
}
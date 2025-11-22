using System;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Domains
{
    public class RemovedReservation: Reservation
    {
        public int ReservationId { get; set; }
        
        public DateTime OnRemovedUtc { get; set; }

        public Reservation GetReservation()
        {
            var reservation = (Reservation)this;
            reservation.Id = ReservationId;
            return reservation;
        }
        
    }
}
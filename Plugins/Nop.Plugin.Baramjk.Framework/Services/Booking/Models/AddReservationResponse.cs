using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Models
{
    public class AddReservationResponse : CamelCaseBaseDto
    {
        public string Error { get; set; }
        public List<Reservation> Reservations { get; set; } = new();
        public bool IsSuccess { get; set; }
    }
}
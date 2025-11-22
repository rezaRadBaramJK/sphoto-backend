using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reservations
{
    public class UpdateReservationApiParams
    {
        public UpdateReservationApiParams()
        {
            Items = new List<UpdateReservationItemApiParams>();
        }

        public List<UpdateReservationItemApiParams> Items { get; set; }
    }
}
using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Tickets
{
    public class TicketReservationDetailsModel : ReservationDetailsModel
    {
        public Customer Customer { get; set; }
        public CashierEvent CashierEvent { get; set; }
        public List<Actor> Actors { get; set; }
        public string MyFatoorahReference { get; set; }
        
        public decimal WalletUsedAmount { get; set; }
    }
}
using System;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class ReservationServiceResult
    {
        public int OrderId { get; set; }
        public int ReservationId { get; set; }
        public string ProductName { get; set; }
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }
        public int? AddressId { get; set; }
        public AddressModel Address { get; set; }
    }
}
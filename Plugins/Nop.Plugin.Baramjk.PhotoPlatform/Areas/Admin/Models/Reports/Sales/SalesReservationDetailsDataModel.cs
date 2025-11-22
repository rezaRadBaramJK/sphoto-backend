namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Sales
{
    public class SalesReservationDetailsDataModel
    {
        public string ActorName { get; set; }
        public int NumberOfTickets { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ProductionShare { get; set; }
        public decimal ActorShare { get; set; }
        public decimal PhotoShootShare { get; set; }
    }
}
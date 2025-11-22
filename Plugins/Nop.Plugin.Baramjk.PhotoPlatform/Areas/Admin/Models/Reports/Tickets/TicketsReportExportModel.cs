using System;
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Tickets
{
    public class TicketsReportExportModel
    {
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string TimeSlot { get; set; }
        public int OrderId { get; set; }
        public string MyFatoorahReference { get; set; }
        public decimal TicketPrice { get; set; }
        public string TicketType { get; set; }
        public decimal VisaFee { get; set; }
        public decimal KNetFee { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal NetPrice { get; set; }
        public string PaymentType { get; set; }
        public decimal WalletUsedAmount { get; set; }
        public int UsedPhotosCount { get; set; }
        public int NotUsedPhotosCount { get; set; }
        public string AccountantName { get; set; }
        public List<TicketsReportActorPartModel> ActorsData { get; set; }
        public string ClientName { get; set; }
        public string ClientPhoneNumber { get; set; }
        public string ClientEmail { get; set; }
    }
}
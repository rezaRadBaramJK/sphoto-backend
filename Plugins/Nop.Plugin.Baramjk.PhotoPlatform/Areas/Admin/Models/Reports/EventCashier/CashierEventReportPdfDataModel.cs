using System;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.EventCashier
{
    public class CashierEventReportPdfDataModel
    {
        public bool IncludeTimeSlot { get; set; }
        public string EventName { get; set; }
        public string CashierName { get; set; }
        public DateTime DayDateTime { get; set; }
        public TimeSpan TimeSlot { get; set; }
        public int TotalCameraManPhotoCount { get; set; }
        public int TotalCustomerMobilePhotoCount { get; set; }
        public decimal TotalCameraManPhotoPrice { get; set; }
        public decimal TotalCustomerMobilePhotoPrice { get; set; }
        public int TotalPhotoCount { get; set; }
        public decimal TotalPhotoPrice { get; set; }
        public decimal TotalRefundedPhotoCount { get; set; }
        public decimal TotalRefundedPrice { get; set; }
        public decimal PhotoCountNetProfit { get; set; }
        public decimal TotalPhotoPriceNetProfit { get; set; }
        public decimal OpeningFundBalance { get; set; }
       
        public decimal TotalCashPayments { get; set; }
        public decimal TotalFundBalance { get; set; }
        public decimal TotalKNetPayments { get; set; }
        public decimal TotalOnlinePayments { get; set; }
        public decimal TotalNetProfit { get; set; }

    }
}
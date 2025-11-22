using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.EventCashier
{
    public class CashierEventReportViewModel
    {
        public CashierEventReportViewModel()
        {
            AvailableEvents = new List<SelectListItem>();
            AvailableDates = new List<SelectListItem>();
            AvailableCashiers = new List<SelectListItem>();
            AvailableTimeSlots = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.CashierEventReportViewModel.EventId")]
        public int EventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.CashierEventReportViewModel.CashierId")]
        public int CashierId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.CashierEventReportViewModel.EventDate")]
        public DateTime EventDate { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.CashierEventReportViewModel.TimeSlotId")]
        public int? TimeSlotId { get; set; }


        public List<SelectListItem> AvailableEvents { get; set; }

        public List<SelectListItem> AvailableDates { get; set; }

        public List<SelectListItem> AvailableCashiers { get; set; }

        public List<SelectListItem> AvailableTimeSlots { get; set; }
    }
}
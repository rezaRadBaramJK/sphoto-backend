using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Cashier
{
    public record CashierReportViewModel : BaseNopEntityModel
    {
        public CashierReportViewModel()
        {
            AvailableEvents = new List<SelectListItem>();
            AvailableDates = new List<SelectListItem>();
            AvailableTimeSlots = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.EventId")]
        public int EventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.EventDate")]
        public DateTime EventDate { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.TimeSlotId")]
        public int TimeSlotId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.ShowTicketsCount")]
        public bool ShowTicketsCount { get; set; }

        public List<SelectListItem> AvailableEvents { get; set; }

        public List<SelectListItem> AvailableDates { get; set; }

        public List<SelectListItem> AvailableTimeSlots { get; set; }
    }
}
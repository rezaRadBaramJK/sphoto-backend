using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Tickets
{
    public class TicketsReportViewModel
    {
        public TicketsReportViewModel()
        {
            AvailableEvents = new List<SelectListItem>();
            AvailableDates = new List<SelectListItem>();
            AvailableTimeSlots = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.EventIds")]
        public IList<int> EventIds { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.FromDate")]
        [UIHint("Date")]
        public DateTime? FromDate { get; set; }

        [UIHint("Date")]
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.ToDate")]
        public DateTime? ToDate { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.TimeSlotStartTime")]
        public TimeSpan? TimeSlotStartTime { get; set; }


        public List<SelectListItem> AvailableEvents { get; set; }

        public List<SelectListItem> AvailableDates { get; set; }

        public List<SelectListItem> AvailableTimeSlots { get; set; }
    }
}
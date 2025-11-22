using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Cashier
{
    public record SalesReportViewModel : BaseNopEntityModel
    {
        public SalesReportViewModel()
        {
            AvailableEvents = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Sales.EventId")]
        public int EventId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Sales.OnlyShowConfirmedTickets")]
        public bool OnlyShowConfirmedTickets { get; set; }


        public List<SelectListItem> AvailableEvents { get; set; }
    }
}
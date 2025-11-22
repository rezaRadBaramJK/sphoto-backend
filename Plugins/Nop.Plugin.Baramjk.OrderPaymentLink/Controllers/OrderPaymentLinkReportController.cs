using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.OrderPaymentLink.Plugins;
using Nop.Plugin.Baramjk.OrderPaymentLink.Services;
using Nop.Plugin.Baramjk.OrderPaymentLink.Services.Model;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class OrderPaymentLinkReportController : BaseBaramjkPluginController
    {
        private readonly InvoiceService _invoiceService;
        private readonly IPermissionService _permissionService;

        public OrderPaymentLinkReportController(InvoiceService invoiceService, IPermissionService permissionService)
        {
            _invoiceService = invoiceService;
            _permissionService = permissionService;
        }

        [HttpGet("/OrderPaymentLinkReport/Report")]
        public async Task<IActionResult> Report([FromQuery] ReportRequest request)
        {
            if (!await _permissionService.AuthorizeAsync(PermissionProvider.Management))
                return AccessDeniedView();

            if (request.From == null)
                request.From = DateTime.Now;
            if (request.To == null)
                request.To = DateTime.Now;

            var translations = await _invoiceService.GetReportAsync(request);
            ViewBag.From = request.From.Value.ToString("yyyy-MM-dd");
            ViewBag.To = request.To.Value.ToString("yyyy-MM-dd");

            return View("OrderPaymentLinkReports/Report.cshtml", translations);
        }


        [HttpPost("/admin/OrderPaymentLinkReport/GetStatus")]
        public async Task<IActionResult> GetStatus([FromBody] GetStatusListRequest request)
        {
            var responses = _invoiceService.GetSentInvoiceState(request);
            return Ok(responses);
        }
    }

    public class ReportRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
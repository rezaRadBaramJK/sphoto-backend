using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents
{
    public class ChangeCashierBalanceViewModel
    {
        public int CashierEventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ChangeCashierBalanceViewModel.ChangeAmount")]
        public decimal ChangeAmount { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.ChangeCashierBalanceViewModel.ChangeType")]

        public int ChangeType { get; set; }

        public IList<SelectListItem> AvailableChangeTypes { get; set; } = new List<SelectListItem>();
    }
}
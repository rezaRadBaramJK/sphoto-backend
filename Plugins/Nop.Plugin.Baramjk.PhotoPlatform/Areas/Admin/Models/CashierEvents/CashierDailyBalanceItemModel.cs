using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents
{
    public record CashierDailyBalanceItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CashierEmail")]
        public string CashierEmail { get; set; }

        [UIHint("Date")]
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.Day")]
        public DateTime Day { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.OpeningFundBalanceAmount")]
        public decimal OpeningFundBalanceAmount { get; set; }
    }
}
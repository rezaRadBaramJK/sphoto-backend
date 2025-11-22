using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents
{
    public record CashierDailyBalanceViewModel : BaseNopEntityModel
    {
        public CashierDailyBalanceViewModel()
        {
            AvailableCashiers = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.EventId")]
        public int EventId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.Cashier")]
        public string CashierEmail { get; set; }


        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.Cashier")]
        public int CashierId { get; set; }

        [UIHint("Date")]
        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.Day")]
        public DateTime Day { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.OpeningFundBalanceAmount")]
        public decimal OpeningFundBalanceAmount { get; set; }

        public IList<SelectListItem> AvailableCashiers { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Wallet.Models.Search
{
    public record HistoriesSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Orders.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }
        
        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }
        
        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchUsername")]
        public string SearchUsername { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Types")]
        public IList<int> HistoryTypeIds { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Currencies")]
        public IList<int> CurrencyIds { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.Note")]
        public string SearchNote { get; set; }
        
        public IList<SelectListItem> AvailableHistoryTypes { get; set; }
        public IList<SelectListItem> AvailableCurrencies { get; set; }


    }
}
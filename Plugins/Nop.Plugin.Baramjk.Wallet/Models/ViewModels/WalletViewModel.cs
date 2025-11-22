using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Wallet.Models.ViewModels
{
    public record WalletViewModel : BaseNopModel
    {
        public int Id { get; set; }
        
        public int CustomerId { get; set; }
        
        public int CurrencyId { get; set; }
        
        public string CurrencyName { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.CurrentAmount")]
        // public decimal CurrentAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        // public decimal ActiveRewardAmount { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.AmountToUpdate")]
        public decimal AmountToUpdate { get; set; }
        
        public SelectList HistoryTypes { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.HistoryType")]
        public int HistoryTypeId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.Widget.Customers.Histories.ExpirationDateTime")]
        public DateTime? ExpirationDate { get; set; }
        
        
    }
}
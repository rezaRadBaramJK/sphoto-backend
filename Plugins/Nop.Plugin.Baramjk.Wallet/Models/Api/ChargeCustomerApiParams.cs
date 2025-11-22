using System;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;

namespace Nop.Plugin.Baramjk.Wallet.Models.Api
{
    public class ChargeCustomerApiParams
    {
        public int CurrencyId { get; set; }
        
        public int CustomerId { get; set; }
        
        public decimal AmountToUpdate { get; set; }
        
        public string ExpirationDate { get; set; }
        
        public int HistoryTypeId { get; set; }
        
    }
}
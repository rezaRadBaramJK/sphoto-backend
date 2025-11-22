using System;
using Nop.Core;

namespace Nop.Plugin.Baramjk.Wallet.Domain
{
    public class WithdrawRequest : BaseEntity
    {
        public int CustomerId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string? CartNumber { get; set; }
        public string? IBAN { get; set; }
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public bool? Status { get; set; }
        public DateTime OnDateTime { get; set; }
    }
}
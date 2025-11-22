using System;

namespace Nop.Plugin.Baramjk.Wallet.Services.Models
{
    public record WithdrawRequestItem(int Id, string Email, int CustomerId, string CurrencyCode, decimal Amount,
        string? CartNumber, string? IBAN, string? AccountNumber, string? BankName, bool? Status, DateTime OnDateTime);
}
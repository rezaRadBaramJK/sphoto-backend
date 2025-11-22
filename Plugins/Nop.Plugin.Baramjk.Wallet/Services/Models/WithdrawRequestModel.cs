namespace Nop.Plugin.Baramjk.Wallet.Services.Models
{
    public class WithdrawRequestModel
    {
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string? CartNumber { get; set; }
        public string? IBAN { get; set; }
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
    }
}
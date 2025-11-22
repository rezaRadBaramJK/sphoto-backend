namespace Nop.Plugin.Baramjk.Framework.Services.Wallets
{
    public class WalletModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal Amount { get; set; }
        public decimal LockAmount { get; set; }
        public decimal AvailableAmount { get; set; }
        public bool IsLocked { get; set; }
        public decimal TotalAmountUsed { get; set; }
        public decimal ExtraCredit { get; set; }
        public decimal RemainingCredit { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal TotalRemainingInPrimary { get; set; }
        public decimal TotalRemainingInCurrent { get; set; }
        public string TotalRemainingDisplay { get; set; }
        public string TotalRemainingInPrimaryDisplay { get; set; }
        public string TotalRemainingInCurrentDisplay { get; set; }
    }
}
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Wallet.Models
{
    public record WalletSettingModel : BaseNopModel
    {
        public string MyFatoorahAccessKey { get; set; }

        public bool MyFatoorahUseSandbox { get; set; }

        public string DefaultCurrencyCode { get; set; }
        public decimal AutoUseWalletAmount { get; set; }
        public decimal AmountForRegistration { get; set; }
        public decimal MinimumAmountToUsePerOrder { get; set; }
        public decimal MaximumAmountToUsePerOrder { get; set; }
        public decimal MinimumOrderTotalToUse { get; set; }
        public decimal EachOrderTotal { get; set; }
        public decimal EachOrderTotalEarn { get; set; }
        public decimal MaximumAmountEarnPerOrder { get; set; }
        
        public bool CashBackEnabled { get; set; }
        public int EarnFromOrderPercent { get; set; }
        public int RewardExpirationDays { get; set; }
        
        public string PublicViewComponentName { get; set; }
        public bool SkipPaymentInfo { get; set; }
        
        public bool ForceUseWalletCredit {get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.IsEnableChargeWallet")]
        public bool IsEnableChargeWallet {get; set; }
        
    }
}
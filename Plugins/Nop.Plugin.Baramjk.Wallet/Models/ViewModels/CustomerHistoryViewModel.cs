using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Wallet.Models.ViewModels
{
    public record CustomerHistoryViewModel : BaseNopEntityModel
    {
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string CreateDateTime { get; set; }
        public string ExpirationDateTime { get; set; }
        public int OriginatedEntityId { get; set; }
        public string Note { get; set; }

    }
}
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Wallet.Models.Search
{
    public record CustomerHistorySearchModel : BaseSearchModel
    {
        public int CustomerId { get; set; }
    }
}
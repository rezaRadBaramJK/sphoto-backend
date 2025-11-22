using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Wallet.Models.ViewModels.Packages
{
    public record PackageListModelItem : BaseNopEntityModel
    {
        public string Name { get; set; }

        public List<PackageAmountValue> AmountValues { get; set; } = new List<PackageAmountValue>();

    }

    public class PackageAmountValue
    {
        public string Title { get; set; }
        public int Id { get; set; }
    }
}
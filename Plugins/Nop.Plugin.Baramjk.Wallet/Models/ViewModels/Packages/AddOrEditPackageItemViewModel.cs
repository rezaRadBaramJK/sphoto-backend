using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Wallet.Models.ViewModels.Packages
{
    public class AddOrEditPackageItemViewModel 
    {
        public int? Id { get; set; }
        public int PackageId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.Packages.CurrencyCode")]
        [Required]
        [MaxLength(255)]
        public string CurrencyCode { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.Packages.Amount")]
        public decimal  Amount { get; set; }
    }
}
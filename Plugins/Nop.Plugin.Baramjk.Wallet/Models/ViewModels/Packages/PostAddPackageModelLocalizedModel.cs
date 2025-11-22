using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Wallet.Controllers
{
    public partial class PostAddOrEditPackageModelLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.Packages.Name")]
        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
    }
}
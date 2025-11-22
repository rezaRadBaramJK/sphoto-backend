using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Baramjk.Wallet.Controllers;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Wallet.Models.ViewModels.Packages
{
    public class AddOrEditPackageViewModel : ILocalizedModel<PostAddOrEditPackageModelLocalizedModel>
    {
        public int? Id { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.Wallet.Admin.Packages.Name")]
        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
        public IList<PostAddOrEditPackageModelLocalizedModel> Locales { get; set; }
    }
}
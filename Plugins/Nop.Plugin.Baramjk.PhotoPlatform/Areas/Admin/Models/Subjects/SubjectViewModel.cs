using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Subjects
{
    public record SubjectViewModel : BaseNopEntityModel, ILocalizedModel<SubjectLocalizedModel>
    {
        public SubjectViewModel()
        {
            Locales = new List<SubjectLocalizedModel>();
        }

        [NopResourceDisplayName("Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.Name")]
        public string Name { get; set; }

        public IList<SubjectLocalizedModel> Locales { get; set; }
    }
}
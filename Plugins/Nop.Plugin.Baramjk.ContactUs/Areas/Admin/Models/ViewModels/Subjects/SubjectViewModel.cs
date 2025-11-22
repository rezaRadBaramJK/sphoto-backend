using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.ViewModels.Subjects
{
    public record SubjectViewModel : BaseNopEntityModel, ILocalizedModel<SubjectLocalizedModel>
    {
        public SubjectViewModel()
        {
            Locales = new List<SubjectLocalizedModel>();
        }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Subjects.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Subjects.IsPayable")]
        public bool IsPayable => Price > 0;
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Subjects.Price")]
        public decimal Price { get; set; }
        
        public IList<SubjectLocalizedModel> Locales { get; set; }
    }
}
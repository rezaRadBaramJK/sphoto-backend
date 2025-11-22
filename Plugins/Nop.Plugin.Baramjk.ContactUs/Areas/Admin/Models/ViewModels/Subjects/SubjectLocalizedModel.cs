using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.ViewModels.Subjects
{
    public class SubjectLocalizedModel: ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }
        
        [NopResourceDisplayName("Baramjk.ContactUs.Admin.Subjects.Name")]
        public string Name { get; set; }
    }
}
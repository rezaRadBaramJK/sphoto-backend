using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.ViewModels.Subjects
{
    public record SubjectSearchModel: BaseSearchModel
    {
        public SubjectViewModel AddNewSubjectModel { get; set; }
    }
}
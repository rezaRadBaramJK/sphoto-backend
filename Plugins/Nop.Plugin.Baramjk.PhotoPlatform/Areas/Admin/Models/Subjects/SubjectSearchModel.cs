using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Subjects
{
    public record SubjectSearchModel: BaseSearchModel
    {
        public SubjectViewModel AddNewSubjectModel { get; set; }
    }
}
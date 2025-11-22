using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Subjects;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class SubjectAdminFactory
    {
        private readonly PhotoPlatformSubjectService _subjectService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;

        public SubjectAdminFactory(
            PhotoPlatformSubjectService subjectService,
            ILanguageService languageService,
            ILocalizationService localizationService)
        {
            _subjectService = subjectService;
            _languageService = languageService;
            _localizationService = localizationService;
        }

        public SubjectSearchModel PrepareSearchModel()
        {
            var model = new SubjectSearchModel();
            model.SetGridPageSize();
            return model;
        }

        public async Task<SubjectListModel> PrepareListModelAsync(SubjectSearchModel searchModel)
        {
            var pageIndex = searchModel.Page - 1;
            var pageSize = searchModel.PageSize;

            var entities = await _subjectService.GetAsync(pageIndex, pageSize);

            return await new SubjectListModel().PrepareToGridAsync(searchModel, entities,
                () =>
                {
                    return entities.SelectAwait(async subject => new SubjectViewModel
                    {
                        Id = subject.Id,
                        Name = await _localizationService.GetLocalizedAsync(subject, s => s.Name),
                    });
                });
        }

        public async Task<SubjectViewModel> PrepareViewModelAsync(SubjectEntity subject = null)
        {
            var availableLanguages = await _languageService.GetAllLanguagesAsync(true);

            if (subject == null)
            {
                return new SubjectViewModel
                {
                    Locales = availableLanguages.Select(language => new SubjectLocalizedModel
                    {
                        LanguageId = language.Id,
                    }).ToList()
                };
            }

            var viewModel = new SubjectViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Locales = await availableLanguages.SelectAwait(async language =>
                {
                    return new SubjectLocalizedModel
                    {
                        LanguageId = language.Id,
                        Name = await _localizationService.GetLocalizedAsync(
                            subject,
                            entity => entity.Name,
                            language.Id,
                            false,
                            false)
                    };
                }).ToListAsync()
            };
            return viewModel;
        }
    }
}
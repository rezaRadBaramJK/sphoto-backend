using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ContactUs.Subject;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Factories.ContactUs
{
    public class SubjectFactory
    {
        private readonly ILocalizationService _localizationService;


        public SubjectFactory(
            ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task<IList<SubjectDto>> PrepareDtoListAsync(IList<SubjectEntity> entities)
        {
            return await entities.SelectAwait(async subject => new SubjectDto
            {
                Id = subject.Id,
                Name = await _localizationService.GetLocalizedAsync(subject, s => s.Name),
            }).ToListAsync();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.ContactUs.Domains;
using Nop.Plugin.Baramjk.ContactUs.Models.Api.Subject;
using Nop.Services.Catalog;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.ContactUs.Factories
{
    public class SubjectFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;

        public SubjectFactory(
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter)
        {
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
        }

        public async Task<IList<SubjectDto>> PrepareDtoListAsync(IList<SubjectEntity> entities)
        {
            return await entities.SelectAwait(async subject => new SubjectDto
            {
                Id = subject.Id,
                Name = await _localizationService.GetLocalizedAsync(subject, s => s.Name),
                Price = subject.Price,
                PriceString = await _priceFormatter.FormatPriceAsync(subject.Price)
            }).ToListAsync();
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Banner.Models;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Banner.Services
{
    public class BannerLocalizationService
    {
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly ILogger _logger;
        
        public BannerLocalizationService(
            IWorkContext workContext,
            ILocalizationService localizationService,
            ILanguageService languageService, ILogger logger)
        {
            _workContext = workContext;
            _localizationService = localizationService;
            _languageService = languageService;
            _logger = logger;
        }

        public async Task UpdateLocalizationAsync(List<BannerRecord> banners)
        {
            var language = await _workContext.GetWorkingLanguageAsync();

            foreach (var b in banners)
            {
                var title = await _localizationService
                    .GetLocalizedAsync(b, entity => entity.Title, language.Id, false, false);

                if (string.IsNullOrEmpty(title) == false)
                    b.Title = title;
                
                var text = await _localizationService
                    .GetLocalizedAsync(b, entity => entity.Text, language.Id, false, false);
                if (string.IsNullOrEmpty(text) == false)
                    b.Text = text;
            }
        }
        
        
        public async Task<IList<BannerLocalizedModel>> PrepareLocalizedModelsAsync(BannerRecord banner)
        {
            var availableLanguages = await _languageService.GetAllLanguagesAsync(true);
            var bannerLocalized = new List<BannerLocalizedModel>();
            foreach (var language in availableLanguages)
            {
                var foodLocalizedModel = new BannerLocalizedModel
                {
                    LanguageId = language.Id,
                    Title = await _localizationService.GetLocalizedAsync(banner, entity => entity.Title, language.Id, false, false),
                    Text = await _localizationService.GetLocalizedAsync(banner, entity => entity.Text, language.Id, false, false)
                };
                bannerLocalized.Add(foodLocalizedModel);
            }
            
            return bannerLocalized;
        }
    }
}
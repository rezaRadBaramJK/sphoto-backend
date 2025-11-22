using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class CustomLocalizationService : LocalizationService
    {
        private readonly ILanguageService _languageService;
        private readonly IRepository<LocaleStringResource> _lsrRepository;
        private readonly IStaticCacheManager _staticCacheManager;


        public CustomLocalizationService(ILanguageService languageService, ILocalizedEntityService localizedEntityService, ILogger logger,
            IRepository<LocaleStringResource> lsrRepository, ISettingService settingService, IStaticCacheManager staticCacheManager,
            IWorkContext workContext, LocalizationSettings localizationSettings) : base(languageService, localizedEntityService, logger,
            lsrRepository, settingService, staticCacheManager, workContext, localizationSettings)
        {
            _languageService = languageService;
            _lsrRepository = lsrRepository;
            _staticCacheManager = staticCacheManager;
        }

        public override async Task AddLocaleResourceAsync(IDictionary<string, string> resources, int? languageId = null)
        {
            var existingResources = new HashSet<string>(
                await _lsrRepository.Table
                    .Where(lsr => lsr.LanguageId == languageId && resources.Keys.Contains(lsr.ResourceName))
                    .Select(lsr => lsr.ResourceName)
                    .ToArrayAsync()
            );

            var newResources = resources
                .Where(kvp => existingResources.Contains(kvp.Key) == false)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var locales = (await _languageService.GetAllLanguagesAsync(true))
                .Where(language => !languageId.HasValue || language.Id == languageId.Value)
                .SelectMany(language => newResources.Select(resource => new LocaleStringResource
                {
                    LanguageId = language.Id,
                    ResourceName = resource.Key,
                    ResourceValue = resource.Value
                }))
                .ToList();

            await _lsrRepository.InsertAsync(locales, false);

            await _staticCacheManager.RemoveByPrefixAsync(NopEntityCacheDefaults<LocaleStringResource>.Prefix);
        }
    }
}
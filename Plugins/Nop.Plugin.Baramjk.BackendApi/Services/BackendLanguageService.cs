using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;

namespace Nop.Plugin.Baramjk.BackendApi.Services
{
    public class BackendLanguageService : LanguageService
    {
        private readonly IRepository<Language> _languageRepository;
        
        public BackendLanguageService(
            IRepository<Language> languageRepository,
            ISettingService settingService, 
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            LocalizationSettings localizationSettings) :
            base(languageRepository, settingService, staticCacheManager, storeMappingService, localizationSettings)
        {
            _languageRepository = languageRepository;
        }

        public Task<Language> GetLanguageByCultureAsync(string languageCulture)
        {
            return 
            _languageRepository.Table
                .FirstOrDefaultAsync(l => l.LanguageCulture == languageCulture);
        }
    }
}
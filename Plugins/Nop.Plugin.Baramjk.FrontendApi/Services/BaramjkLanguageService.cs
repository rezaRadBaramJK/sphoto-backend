using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.Language;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class BaramjkLanguageService :IBaramjkLanguageService
    {
        private readonly IRepository<Language> _repository;

        public BaramjkLanguageService(IRepository<Language> repository)
        {
            _repository = repository;
        }

        public async Task<int> GetEnglishLanguageId()
        {
            return await GetLanguageIdByIsoCode("en");
        }

        public async Task<int> GetArabicLanguageId()
        {
            return await GetLanguageIdByIsoCode("ar");
        }

        public async Task<int> GetLanguageIdByIsoCode(string isoCode)
        {
            var lang = await _repository.Table.Where(x => x.UniqueSeoCode == isoCode).FirstOrDefaultAsync();
            return lang?.Id ?? 0;
        }
    }
}
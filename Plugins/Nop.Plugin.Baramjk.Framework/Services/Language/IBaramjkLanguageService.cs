using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.Language
{
    public interface IBaramjkLanguageService
    {
        public Task<int> GetEnglishLanguageId();
        public Task<int> GetArabicLanguageId();
        public Task<int> GetLanguageIdByIsoCode(string isoCode);
    }
}
using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.DataBaseUtils
{
    public interface IDatabaseTableUtilsService
    {
        Task<bool> CheckTableExistAsync(string name);
        Task<int> DeleteTableIfExistAsync(string name);
    }
}
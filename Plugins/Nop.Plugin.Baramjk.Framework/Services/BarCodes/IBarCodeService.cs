using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.BarCodes
{
    public interface IBarCodeService
    {
        Task<string> CreateCode128Async(string code);
        Task<string> CreateQRCodeAsync(string code);
    }
}
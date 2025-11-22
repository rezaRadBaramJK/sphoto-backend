using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.License.Models.DemoLicenseServices;
using Nop.Plugin.Baramjk.Framework.Services.Networks;

namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public interface IDemoLicenseService
    {
        Task<HttpResponse<DemoLicenseResponse>> CreateDemoPluginLicenseAsync(string pluginName);
    }
}
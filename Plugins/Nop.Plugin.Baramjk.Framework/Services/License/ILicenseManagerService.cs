using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Services.License.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public interface ILicenseManagerService
    {
        List<PluginLicenseRecord> GetPluginLicenses();
        PluginLicense GetPluginLicense(string pluginName);
        Task SaveLicense(string pluginName, PluginLicense pluginLicense);
        Task DeleteByToken(string token);
        Task Delete(int id);
        Task SaveLicense(string pluginName, string license);
    }
}
using Nop.Plugin.Baramjk.Framework.Services.License.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public interface ILicenseParser
    {
        PluginLicense Pars(string license);
        bool TryPars(string license, out PluginLicense pluginLicense);
    }
}
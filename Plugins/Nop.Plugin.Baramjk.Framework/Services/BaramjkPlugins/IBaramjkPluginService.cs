using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Plugins;

namespace Nop.Plugin.Baramjk.Framework.Services.BaramjkPlugins
{
    public interface IBaramjkPluginService
    {
        Task<IList<PluginDescriptor>> GetBaramjkPluginDescriptorsAsync(
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly);
    }
}
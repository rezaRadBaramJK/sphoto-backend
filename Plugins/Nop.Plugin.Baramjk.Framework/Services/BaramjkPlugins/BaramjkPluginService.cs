using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Plugins;

namespace Nop.Plugin.Baramjk.Framework.Services.BaramjkPlugins
{
    public class BaramjkPluginService : IBaramjkPluginService
    {
        private readonly IPluginService _pluginService;

        public BaramjkPluginService(IPluginService pluginService)
        {
            _pluginService = pluginService;
        }

        public async Task<IList<PluginDescriptor>> GetBaramjkPluginDescriptorsAsync(
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
        {
            var pluginDescriptors = await _pluginService.GetPluginDescriptorsAsync<IPlugin>(loadMode);
            pluginDescriptors = pluginDescriptors
                .Where(item => item.Group.Contains("baramjk", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return pluginDescriptors;
        }
    }
}
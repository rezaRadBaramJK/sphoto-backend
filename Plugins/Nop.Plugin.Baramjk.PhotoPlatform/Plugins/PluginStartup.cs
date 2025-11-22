using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Rotativa.AspNetCore;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Plugins
{
    public class PluginStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public void Configure(IApplicationBuilder application)
        {
            var rotativaPath = Path.Combine($"Plugins/{DefaultValues.SystemName}");

            RotativaConfiguration.Setup(rotativaPath);
        }

        public int Order => 1;
    }
}
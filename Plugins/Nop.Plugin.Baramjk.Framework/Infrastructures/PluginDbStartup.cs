using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Baramjk.Framework.Infrastructures
{
    public class PluginDbStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
           
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 1;

    }

}
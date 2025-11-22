using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Plugin
{
    public class PluginStartUp : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var mvcBuilder = services.AddControllersWithViews();
            mvcBuilder.AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 1;
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using HttpClientBuilderExtensions = Nop.Web.Framework.Infrastructure.Extensions.HttpClientBuilderExtensions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins
{
    public class PluginNopStartup : INopStartup
    {
        public int Order => 11;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            HttpClientBuilderExtensions.WithProxy(services.AddHttpClient<MyFatoorahHttpClient>());
            services.Configure(delegate(RazorViewEngineOptions options) { options.ViewLocationExpanders.Add(new ViewLocationExpander()); });
        }

        public void Configure(IApplicationBuilder application)
        {
        }
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Middleware;

namespace Nop.Plugin.Baramjk.FrontendApi.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc()
                .AddNewtonsoftJson(opts =>
                {
                    opts.SerializerSettings.Converters.Add(new StringEnumConverter());
                    // opts.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    opts.SerializerSettings.DateFormatString = "yyyy/MM/dd HH:mm:ss";
                });
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseRouting();

            // global error handler
            application.UseMiddleware<ErrorHandlerMiddleware>();

            // custom jwt auth middleware
            application.UseMiddleware<JwtMiddleware>();

            application.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        public int Order => 1;
    }
}
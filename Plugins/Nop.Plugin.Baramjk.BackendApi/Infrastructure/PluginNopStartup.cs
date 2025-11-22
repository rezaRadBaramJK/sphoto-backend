using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.BackendApi.Framework.Middleware;

namespace Nop.Plugin.Baramjk.BackendApi.Infrastructure
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

            AddCors(services);
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseCors("baramjk");
            // global cors policy


            application.UseRouting();

            // global error handler
            application.UseMiddleware<ErrorHandlerMiddleware>();

            // custom jwt auth middleware
            application.UseMiddleware<JwtMiddleware>();

            application.UseMiddleware<LanguageMiddleware>();

            application.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }


        private void AddCors(IServiceCollection services)
        {
            var allowedOrigins = new List<string>();
            var allowedOriginStr = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
            if (!string.IsNullOrEmpty(allowedOriginStr))
            {
                allowedOrigins = allowedOriginStr.Split(",").ToList();
            }
            
            if (System.IO.File.Exists("./cors.txt"))
            {
                var corsFile = System.IO.File.ReadAllText("./cors.txt");
                if (!string.IsNullOrEmpty(corsFile))
                {
                    allowedOrigins = corsFile.Split(",").ToList();
                }
            }


            if (allowedOrigins.Any())
            {
                services.AddCors(x =>
                {
                    x.AddPolicy("baramjk", builder =>
                    {
                        builder.WithOrigins(allowedOrigins.ToArray())
                            .AllowCredentials()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });
            }
            else
            {
                services.AddCors(x =>
                {
                    x.AddPolicy("baramjk", builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });
            }
        }

        public int Order => 1;
    }
}
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nop.Services.Configuration;

namespace Nop.Web
{
    public class Program
    {
        /// <returns>A task that represents the asynchronous operation</returns>
        public static async Task Main(string[] args)
        {
            //initialize the host
            using var host = Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging(loggerBuilder =>
                {
                    loggerBuilder
                        .ClearProviders()
                        // Example output: [20/11/20 14:31:30:409] info: ...
                        .AddConsole(configure => configure.TimestampFormat = "[dd/MM/yy HH:mm:ss:fff] "); 
                
                })
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .ConfigureAppConfiguration(config =>
                    {
                        config
                            .AddJsonFile(NopConfigurationDefaults.AppSettingsFilePath, true, true)
                            .AddEnvironmentVariables();
                    })
                    .UseStartup<Startup>())
                .Build();

            //start the program, a task will be completed when the host starts
            await host.StartAsync();

            //a task will be completed when shutdown is triggered
            await host.WaitForShutdownAsync();
        }
    }
}
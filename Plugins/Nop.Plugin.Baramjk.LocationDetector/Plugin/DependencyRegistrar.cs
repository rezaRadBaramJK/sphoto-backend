using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.LocationDetector.Services;
using Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces;

namespace Nop.Plugin.Baramjk.LocationDetector.Plugin
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<ILocationDetector, IP2LocationLocationDetector>();
            services.AddScoped<ILocationCurrencyService, LocationCurrencyService>();
            services.AddScoped<IWorkContext, SetCurrencyWebWorkContext>();
            services.AddScoped<ICustomerCurrencyResolverService, CustomerCurrencyResolverService>();
        }

        public int Order => 10;
    }
}
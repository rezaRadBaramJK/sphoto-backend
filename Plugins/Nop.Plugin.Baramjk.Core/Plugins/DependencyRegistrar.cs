using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Core.Factories;

namespace Nop.Plugin.Baramjk.Core.Plugins
{
    public class DependencyRegistrar: IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //factories
            services.AddTransient<VendorAdminFactory>();
        }

        public int Order => 1;
    }
}
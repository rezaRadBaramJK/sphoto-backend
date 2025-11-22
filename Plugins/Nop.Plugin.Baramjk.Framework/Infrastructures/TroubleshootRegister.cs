using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Framework.Services.Troubleshoots;

namespace Nop.Plugin.Baramjk.Framework.Infrastructures
{
    public class AutoDependencyRegister : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            AutoRegister<ITroubleshoot>(services, typeFinder);
        }

        private static void AutoRegister<TFind>(IServiceCollection services, ITypeFinder typeFinder)
        {
            var types = typeFinder.FindClassesOfType<TFind>();
            foreach (var type in types)
                services.AddScoped(typeof(TFind), type);
        }

        public int Order => int.MaxValue;
    }
}
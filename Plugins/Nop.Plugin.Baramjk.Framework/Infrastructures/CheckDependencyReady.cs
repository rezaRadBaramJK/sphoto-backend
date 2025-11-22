using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Baramjk.Framework.Events.Infrastructures;

namespace Nop.Plugin.Baramjk.Framework.Infrastructures
{
    public class CheckDependencyReady : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
                    if (eventPublisher == null)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    await eventPublisher.PublishAsync(new DependencyReadyEvent());
                    break;
                }

                return Task.CompletedTask;
            });
        }

        public int Order => int.MaxValue;
    }
}

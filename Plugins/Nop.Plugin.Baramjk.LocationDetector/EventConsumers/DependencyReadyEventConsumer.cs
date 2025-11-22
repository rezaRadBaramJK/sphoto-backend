using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Events.Infrastructures;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.LocationDetector.EventConsumers
{
    public class DependencyReadyEventConsumer : IConsumer<DependencyReadyEvent>
    {
        private readonly IDispatcherService _dispatcherService;

        public DependencyReadyEventConsumer(IDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
        }

        public Task HandleEventAsync(DependencyReadyEvent eventMessage)
        {
            _dispatcherService.AddConsumer(DefaultValue.CustomerCountryChangedEventName,
                CustomerCountryChangedHandler);
            return Task.CompletedTask;
        }

        private static async Task CustomerCountryChangedHandler(object value)
        {
            if (value is not Dictionary<int, int> mapping || mapping.Count == 0)
                return;

            var (customerId, countryId) = mapping.First();

            await EngineContext.Current.Resolve<ICustomerCurrencyResolverService>().SetCustomerCurrencyByCountryAsync(customerId, countryId);
            
        }
    }
}
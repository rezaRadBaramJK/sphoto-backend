using System;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Events.Infrastructures;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.Framework.EventConsume
{
    public class DependencyReadyEventExampleConsumer : IConsumer<DependencyReadyEvent>
    {
        private readonly IDispatcherService _dispatcherService;

        public DependencyReadyEventExampleConsumer(IDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
        }

        public async Task HandleEventAsync(DependencyReadyEvent eventMessage)
        {
            _dispatcherService.AddConsumer("TestBus", (d) =>
            {
                Console.WriteLine(d);
                return Task.CompletedTask;
            });
            await _dispatcherService.PublishAsync("TestBus", "test bus value");

            _dispatcherService.AddHandler("TestBusHandler", (d) =>
            {
                var temp = ((string)d) + "+++ +++ +++ +++";
                return Task.FromResult<object>(temp);
            });

            await foreach (var item in _dispatcherService.HandlesAsync<string>("TestBusHandler", "Test Bus Handler"))
            {
                Console.WriteLine(item);
            }

            var result = await _dispatcherService.HandleAsync<string>("TestBusHandler", "Test Bus Handler 2 ");
            Console.WriteLine(result);
        }
    }
}
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Events.Infrastructures;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.PhotoPlatform.EventConsumers
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
            _dispatcherService.AddConsumer(DefaultValues.CustomMoveShoppingCartItemsEvent,
                PhotoPlatformMoveShoppingCartItemsToOrderItemsAsync);
            return Task.CompletedTask;
        }

        private static async Task PhotoPlatformMoveShoppingCartItemsToOrderItemsAsync(object value)
        {
            if (value is not Order order)
                return;

            await EngineContext.Current.Resolve<PhotoPlatformOrderService>().MoveShoppingCartItemsToOrderItemsAsync(order);
        }
    }
}
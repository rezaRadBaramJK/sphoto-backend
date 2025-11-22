using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.PhotoPlatform.EventConsumers
{
    public class OrderEventConsumer : IConsumer<OrderPaidEvent>

    {
        public Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            return EngineContext.Current.Resolve<PhotoPlatformSmsService>().SendTicketSmsAsync(eventMessage.Order);
        }
    }
}
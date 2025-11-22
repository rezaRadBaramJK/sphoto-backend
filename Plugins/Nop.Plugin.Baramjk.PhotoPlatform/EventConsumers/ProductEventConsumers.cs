using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Catalog;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.PhotoPlatform.EventConsumers
{
    public class ProductEventConsumers : IConsumer<EntityDeletedEvent<Product>>, IConsumer<EntityInsertedEvent<Product>>
    {
        private readonly IProductService _productService;
        private readonly TimeSlotService _timeSlotService;

        public ProductEventConsumers(TimeSlotService timeSlotService, IProductService productService)
        {
            _timeSlotService = timeSlotService;
            _productService = productService;
        }

        public Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            return _timeSlotService.DeleteByEventIdAsync(eventMessage.Entity.Id);
        }

        public Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
        {
            eventMessage.Entity.IsShipEnabled = false;
            return _productService.UpdateProductAsync(eventMessage.Entity);
        }
    }
}
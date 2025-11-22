using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Events.Infrastructures;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.Banner.Plugins
{
    public class DependencyReadyEventConsumer : IConsumer<DependencyReadyEvent>
    {
        private readonly IDispatcherService _dispatcherService;
        private readonly IEntityAttachmentService _entityAttachmentService;
        private readonly ILogger _logger;
        private readonly BannerCopyProductService _bannerCopyProductService;
        

        public DependencyReadyEventConsumer(
            IDispatcherService dispatcherService,
            IEntityAttachmentService entityAttachmentService,
            ILogger logger,
            BannerCopyProductService bannerCopyProductService)
        {
            _dispatcherService = dispatcherService;
            _entityAttachmentService = entityAttachmentService;
            _logger = logger;
            _bannerCopyProductService = bannerCopyProductService;
        }

        public Task HandleEventAsync(DependencyReadyEvent eventMessage)
        {
            _dispatcherService.AddConsumer("VendorDto", VendorDtoEventConsumerAsync);
            _dispatcherService.AddConsumer("CopyProduct", CopyProductEventConsumerAsync);
            return Task.CompletedTask;
        }

        private async Task VendorDtoEventConsumerAsync(object value)
        {
            if (value is not DtoBase dtoBase)
                return;

            var models = await _entityAttachmentService.GetAttachmentsAsync("Vendor", dtoBase.Id);
            dtoBase.AddCustomProperty("Banners", models);
        }

        private async Task CopyProductEventConsumerAsync(object value)
        {
            if (value is not string valueString)
            {
                await _logger.ErrorAsync("CopyProductEventConsumer - invalid type of value, it should be string.");
                return;
            }
                

            var valueArray = valueString.Split(",");
            if (valueArray.Length != 2)
            {
                await _logger.ErrorAsync("CopyProductEventConsumer - invalid value string, the value should contain of 2 product ids and seperated by ','.");
                return;
            }

            if (int.TryParse(valueArray[0], out var oldProductId) == false || oldProductId <= 0)
            {
                await _logger.ErrorAsync("CopyProductEventConsumer - invalid old product id value.");
                return;
            }

            if (int.TryParse(valueArray[1], out var newProductId) == false || newProductId <= 0)
            {
                await _logger.ErrorAsync("CopyProductEventConsumer - invalid new product id value.");
                return;
            }

            await _bannerCopyProductService.CopyProductBannersAsync(oldProductId, newProductId);
        }
        
        
    }
}
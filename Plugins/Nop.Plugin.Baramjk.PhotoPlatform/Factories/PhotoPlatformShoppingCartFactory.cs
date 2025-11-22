using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ShoppingCart;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.ShoppingCart;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Factories
{
    public class PhotoPlatformShoppingCartFactory
    {
        private readonly PhotoPlatformShoppingCartService _photoPlatformShoppingCartService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly EventFactory _eventFactory;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;

        public PhotoPlatformShoppingCartFactory(PhotoPlatformShoppingCartService photoPlatformShoppingCartService,
            IPriceFormatter priceFormatter,
            EventFactory eventFactory,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService)
        {
            _photoPlatformShoppingCartService = photoPlatformShoppingCartService;
            _priceFormatter = priceFormatter;
            _eventFactory = eventFactory;
            _currencyService = currencyService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
        }

        private List<ShoppingCartActorPhotoDetailsDto> PreparePhotographyDetails(List<ShoppingCartApiModel> shoppingCartApiItems)
        {
            return shoppingCartApiItems.Select(sci => new ShoppingCartActorPhotoDetailsDto

                {
                    Id = sci.ShoppingCartItemTimeSlot.Id,
                    ActorId = sci.Actor.Id,
                    ActorName = sci.Actor.Name,
                    CustomerMobilePhotoCount = sci.ShoppingCartItemTimeSlot.CustomerMobilePhotoCount,
                    CameraManPhotoCount = sci.ShoppingCartItemTimeSlot.CameraManPhotoCount,
                }
            ).ToList();
        }

        public async Task<ShoppingCartDto> PrepareShoppingCartAsync()
        {
            var shoppingCartDetails = await _photoPlatformShoppingCartService.GetShoppingCartAsync();
            var groupedItems = shoppingCartDetails
                .GroupBy(x => new { ProductId = x.Product.Id, TimeSlotId = x.TimeSlot.Id })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.TimeSlotId,
                    Items = g.ToList()
                }).ToList();


            var customer = await _workContext.GetCurrentCustomerAsync();

            var customerCurrencyId = await _genericAttributeService
                .GetAttributeAsync<int>(customer, NopCustomerDefaults.CurrencyIdAttribute);

            var currency = customerCurrencyId == 0
                ? await _workContext.GetWorkingCurrencyAsync()
                : await _currencyService.GetCurrencyByIdAsync(customerCurrencyId);

            var convertedToUserCurrency =
                await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                    await _photoPlatformShoppingCartService.CalculateShoppingCartTotalPriceAsync(), currency);


            var shoppingCartDto = new ShoppingCartDto
            {
                TotalPrice = await _priceFormatter.FormatPriceAsync(convertedToUserCurrency, true, currency),
                Items = await groupedItems.SelectAwait(async group =>
                {
                    var firstItem = group.Items.First(item => item.Product.Id == group.ProductId);
                    var timeSlotItem = group.Items.First(item => item.TimeSlot.Id == group.TimeSlotId);
                    return new ShoppingCartDetailsDto
                    {
                        Id = firstItem.ShoppingCartItem.Id,
                        EventId = group.ProductId,
                        EventName = firstItem.Product.Name,
                        TimeSlotId = group.TimeSlotId,
                        Picture = await _eventFactory.PrepareEventPictureAsync(firstItem.Product),
                        ReservationDate = timeSlotItem.TimeSlot.Date.ToString("yyyy/MM/dd"),
                        ReservationTime = timeSlotItem.TimeSlot.StartTime.ToString(""),
                        PhotographyDetails = PreparePhotographyDetails(group.Items)
                    };
                }).ToListAsync()
            };
            return shoppingCartDto;
        }
    }
}
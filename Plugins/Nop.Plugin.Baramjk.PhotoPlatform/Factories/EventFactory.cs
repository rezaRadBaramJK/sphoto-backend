using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Models.PagedLists;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.ActorEvent;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Event;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Picture;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.TimeSlot;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using NopModelCacheDefaults = Nop.Web.Infrastructure.Cache.NopModelCacheDefaults;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Factories
{
    public class EventFactory
    {
        private readonly MediaSettings _mediaSettings;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly ActorService _actorService;
        private readonly CashierBalanceService _cashierBalanceService;
        private readonly ISettingService _settingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICurrencyService _currencyService;

        public EventFactory(MediaSettings mediaSettings,
            IStaticCacheManager staticCacheManager,
            IPictureService pictureService,
            IWorkContext workContext,
            IWebHelper webHelper,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            ActorService actorService,
            CashierBalanceService cashierBalanceService,
            ISettingService settingService,
            IGenericAttributeService genericAttributeService,
            ICurrencyService currencyService)


        {
            _mediaSettings = mediaSettings;
            _staticCacheManager = staticCacheManager;
            _pictureService = pictureService;
            _workContext = workContext;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _actorService = actorService;
            _cashierBalanceService = cashierBalanceService;
            _settingService = settingService;
            _genericAttributeService = genericAttributeService;
            _currencyService = currencyService;
        }

        private async Task<PictureDto> PreparePictureModelAsync(Picture picture, int pictureSize)
        {
            string fullSizeImageUrl, imageUrl;
            (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
            (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

            return new PictureDto()
            {
                Id = picture?.Id ?? 0,
                ImageUrl = imageUrl,
                FullSizeImageUrl = fullSizeImageUrl,
            };
        }

        public async Task<List<PictureDto>> PrepareActorPicturesAsync(Actor actor, int? pictureSize = null)

        {
            if (actor == null)
                throw new ArgumentNullException(nameof(actor));

            var defaultPictureSize = pictureSize ?? _mediaSettings.AvatarPictureSize;


            var actorPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                DefaultValues.ActorPictureModelKey,
                actor,
                defaultPictureSize,
                false,
                _webHelper.IsCurrentConnectionSecured());

            var pictures = await _actorService.GetPicturesByActorIdAsync(actor.Id);

            var pictureDtos = await _staticCacheManager.GetAsync(actorPictureCacheKey, async () =>
            {
                var cachedPictures = await pictures.SelectAwait(async picture => await PreparePictureModelAsync(picture, defaultPictureSize))
                    .ToListAsync();

                return cachedPictures;
            });

            return pictureDtos;
        }

        private async Task<bool> IsInWishListAsync(Product product)
        {
            var wishlistItems =
                await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist,
                    productId: product.Id);
            return wishlistItems.Any();
        }

        private async Task<List<PictureDto>> PrepareEventDetailsPictureModelAsync(Product product,
            int? productPictureSize = null)
        {
            var pictureSize = productPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopModelCacheDefaults.ProductDetailsPicturesModelKey,
                product,
                pictureSize,
                true,
                await _workContext.GetWorkingLanguageAsync(),
                _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync()
            );

            var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);

            var pictureModels = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var cachedPictures = await pictures.SelectAwait(async picture => await PreparePictureModelAsync(picture, pictureSize)).ToListAsync();

                return cachedPictures;
            });

            return pictureModels;
        }

        public async Task<PictureDto> PrepareEventPictureAsync(Product product, int? productPictureSize = null)
        {
            var pictureSize = productPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopModelCacheDefaults.ProductDefaultPictureModelKey,
                product,
                pictureSize,
                true,
                await _workContext.GetWorkingLanguageAsync(),
                _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync()
            );

            var defaultPictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                return await PreparePictureModelAsync(picture, pictureSize);
            });

            return defaultPictureModel;
        }

        private static int CalculateEventStatusSituation(DateTime startDateTime, DateTime endDateTime)
        {
            var now = DateTime.Now;

            if (now < startDateTime)
                return (int)EventStatus.Upcoming;

            if (now > endDateTime)
                return (int)EventStatus.Finished;

            return (int)EventStatus.Started;
        }

        private async Task<EventBriefDto> PrepareEventBriefDtoAsync(Product productEvent, EventDetail eventDetail)
        {
            var eventDto = eventDetail.Map<EventBriefDto>();
            eventDto.Id = productEvent.Id;
            eventDto.Name = await _localizationService.GetLocalizedAsync(productEvent, p => p.Name);
            eventDto.Description = productEvent.ShortDescription;
            eventDto.PictureModel = await PrepareEventPictureAsync(productEvent);
            eventDto.IsInWishList = await IsInWishListAsync(productEvent);

            return eventDto;
        }

        private async Task<(decimal, string)> PreparePriceAsync(Customer customer, decimal amount, bool convertCurrency = false)
        {
            if (!convertCurrency) return (amount, await _priceFormatter.FormatPriceAsync(amount));

            var customerCurrencyId = await _genericAttributeService
                .GetAttributeAsync<int>(customer, NopCustomerDefaults.CurrencyIdAttribute);


            var currency = customerCurrencyId == 0
                ? await _workContext.GetWorkingCurrencyAsync()
                : await _currencyService.GetCurrencyByIdAsync(customerCurrencyId);


            var convertedToUserCurrency = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount, currency);

            return (convertedToUserCurrency, await _priceFormatter.FormatPriceAsync(convertedToUserCurrency, true, currency));
        }

        public async Task<List<ActorEventDto>> PrepareActorEventDtosAsync(IEnumerable<EventDetailActor> actorEvents, bool convertCurrency = false)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return await actorEvents.SelectAwait(async ae => new ActorEventDto
            {
                ActorId = ae.Actor.Id,
                ActorName = await _localizationService.GetLocalizedAsync(ae.Actor, a => a.Name),
                CameraManEachPictureCost = (await PreparePriceAsync(customer, ae.CameraManEachPictureCost, convertCurrency)).Item1,
                CustomerMobileEachPictureCost = (await PreparePriceAsync(customer, ae.CustomerMobileEachPictureCost, convertCurrency)).Item1,
                Pictures = await PrepareActorPicturesAsync(ae.Actor),
            }).ToListAsync();
        }


        private static string PrepareDetailedLabel(DateTime date, Language language)
        {
            var culture = new CultureInfo(language.LanguageCulture ?? "en-US");

            var dayName = culture.DateTimeFormat.GetDayName(date.DayOfWeek);
            var monthName = culture.DateTimeFormat.GetMonthName(date.Month);

            if (culture.TwoLetterISOLanguageName == "ar")
            {
                return $"{dayName} {date.Day} {monthName}";
            }

            var abbreviatedDay = culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek);
            return $"{abbreviatedDay} {date.Day} {monthName}";
        }

        private static string TimeSlotLabelGenerator(DateTime dateTime, TimeSlotLabelType type, string dayLabel, Language language, int index = 0)
        {
            return type switch
            {
                TimeSlotLabelType.Brief => $"{dayLabel} {index + 1}",
                TimeSlotLabelType.Detailed => PrepareDetailedLabel(dateTime, language),
                _ => $" {dayLabel} {index + 1}"
            };
        }

        public async Task<List<GroupedTimeSlotsDto>> PrepareGroupedTimeSlotsDtosAsync(IEnumerable<TimeSlot> timeSlots,
            TimeSlotLabelType? labelType = null, bool includePassedTimeSlots = true)
        {
            var dayLabel = await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Api.GroupedTimeSlots.DayLabel");
            var settings = await _settingService.LoadSettingAsync<PhotoPlatformSettings>();
            var language = await _workContext.GetWorkingLanguageAsync();

            var grouped = new List<GroupedTimeSlotsDto>();

            var groupedTimeSlots = timeSlots
                .OrderBy(ts => ts.Date)
                .GroupBy(ts => ts.Date.Date)
                .ToList();

            var index = 0;

            foreach (var g in groupedTimeSlots)
            {
                var timeSlotDtos = await g
                    .Where(ts => includePassedTimeSlots || ts.Date.Date >= DateTime.Now.Date)
                    .SelectAwait(async t =>
                    {
                        var dto = t.Map<TimeSlotDto>();
                        dto.Note = await _localizationService.GetLocalizedAsync(t, tst => dto.Note);
                        return dto;
                    })
                    .ToListAsync();

                grouped.Add(new GroupedTimeSlotsDto
                {
                    Label = TimeSlotLabelGenerator(
                        g.First().Date.Date,
                        labelType ?? (TimeSlotLabelType)settings.TimeSlotLabelTypeId,
                        dayLabel,
                        language,
                        index),
                    Date = g.Key,
                    TimeSlots = timeSlotDtos
                });

                index++;
            }

            return grouped;
        }

        public async Task<EventDto> PrepareEventDtoAsync(EventFullDetails eventDetails, bool includePassedTimeSlots = true,
            bool convertCurrency = false)
        {
            if (eventDetails == null)
                return null;

            var actorEventDtos = await PrepareActorEventDtosAsync(eventDetails.Actors, convertCurrency);
            var groupedTimeSlots = await PrepareGroupedTimeSlotsDtosAsync(eventDetails.TimeSlots, includePassedTimeSlots: includePassedTimeSlots);
            var eventDto = eventDetails.EventDetail.Map<EventDto>();

            eventDto.Id = eventDetails.EventDetail.EventId;
            eventDto.Name = await _localizationService.GetLocalizedAsync(eventDetails.Product, p => p.Name);
            eventDto.Note = await _localizationService.GetLocalizedAsync(eventDetails.EventDetail, ed => ed.Note);
            eventDto.LocationUrlTitle = await _localizationService.GetLocalizedAsync(eventDetails.EventDetail, ed => ed.LocationUrlTitle);
            eventDto.Description = await _localizationService.GetLocalizedAsync(eventDetails.Product, p => p.ShortDescription);
            eventDto.TermsAndConditions = await _localizationService.GetLocalizedAsync(eventDetails.EventDetail, ed => ed.TermsAndConditions);
            eventDto.GroupedTimeSlots = groupedTimeSlots;
            eventDto.ActorsDetails = actorEventDtos;
            eventDto.PictureModel = await PrepareEventDetailsPictureModelAsync(eventDetails.Product);
            eventDto.IsInWishList = await IsInWishListAsync(eventDetails.Product);
            return eventDto;
        }

        public async Task<SupervisorEventDto> PrepareSupervisorEventDtoAsync(EventFullDetails eventDetails, bool includePassedTimeSlots = true)
        {
            if (eventDetails == null)
                return null;

            var actorEventDtos = await PrepareActorEventDtosAsync(eventDetails.Actors);
            var openTimeSlots = await PrepareGroupedTimeSlotsDtosAsync(eventDetails.TimeSlots.Where(ts => ts.Active),
                includePassedTimeSlots: includePassedTimeSlots);
            var closedTimeSlots = await PrepareGroupedTimeSlotsDtosAsync(eventDetails.TimeSlots.Where(ts => ts.Active == false),
                includePassedTimeSlots: includePassedTimeSlots);
            var eventDto = eventDetails.EventDetail.Map<SupervisorEventDto>();

            eventDto.Id = eventDetails.EventDetail.EventId;
            eventDto.Name = await _localizationService.GetLocalizedAsync(eventDetails.Product, p => p.Name);
            eventDto.Description = await _localizationService.GetLocalizedAsync(eventDetails.Product, p => p.ShortDescription);
            eventDto.TermsAndConditions = await _localizationService.GetLocalizedAsync(eventDetails.EventDetail, ed => ed.TermsAndConditions);
            eventDto.OpenTimeSlots = openTimeSlots;
            eventDto.ClosedTimeSlots = closedTimeSlots;
            eventDto.ActorsDetails = actorEventDtos;
            eventDto.PictureModel = await PrepareEventDetailsPictureModelAsync(eventDetails.Product);
            eventDto.IsInWishList = await IsInWishListAsync(eventDetails.Product);
            return eventDto;
        }

        public async Task<List<EventBriefDto>> PrepareEventsAsync(List<ProductEvent> productEvents)
        {
            return await productEvents.SelectAwait(async pe => await PrepareEventBriefDtoAsync(pe.Product, pe.EventDetail)).ToListAsync();
        }

        public async Task<CamelCasePagedList<EventBriefDto>> PrepareEventsPagedListAsync(List<ProductEvent> productEvents, int pageIndex = 1,
            int pageSize = 20, int totalCount = 0)
        {
            var preparedEvents = await productEvents.SelectAwait(async pe => await PrepareEventBriefDtoAsync(pe.Product, pe.EventDetail))
                .ToListAsync();
            return new CamelCasePagedList<EventBriefDto>(
                preparedEvents,
                pageIndex + 1,
                pageSize,
                totalCount);
        }


        public async Task<List<BookmarkedEventDto>> PrepareBookMarkedEventsAsync(List<BookmarkedEventModel> productEvents)
        {
            return await productEvents.SelectAwait(async productEvent =>
            {
                var eventDto = productEvent.EventDetail.Map<BookmarkedEventDto>();
                eventDto.Id = productEvent.Product.Id;
                eventDto.Name = await _localizationService.GetLocalizedAsync(productEvent.Product, p => p.Name);
                eventDto.Description = await _localizationService.GetLocalizedAsync(productEvent.Product, p => p.ShortDescription);
                eventDto.PictureModel = await PrepareEventPictureAsync(productEvent.Product);
                eventDto.IsInWishList = true;
                eventDto.ShoppingCartItemId = productEvent.ShoppingCartItem.Id;

                return eventDto;
            }).ToListAsync();
        }


        public async Task<CashierEventDto> PrepareCashierEventAsync(EventFullDetails eventDetails, Customer customer = null)
        {
            var balance = await _cashierBalanceService.GetBalanceAsync(eventDetails.CashierEvent.Id);
            customer ??= await _workContext.GetCurrentCustomerAsync();
            var baseEventDetailsDto = await PrepareEventDtoAsync(eventDetails);
            var cashierEventDto = baseEventDetailsDto.Map<CashierEventDto>();

            cashierEventDto.EventStatusId = CalculateEventStatusSituation(eventDetails.EventDetail.StartDate + eventDetails.EventDetail.StartTime,
                eventDetails.EventDetail.EndDate + eventDetails.EventDetail.EndTime);
            cashierEventDto.OpeningFundBalanceAmount = balance;
            cashierEventDto.OpeningFundBalance = await _priceFormatter.FormatPriceAsync(balance);
            cashierEventDto.CashierName = await _customerService.GetCustomerFullNameAsync(customer);
            return cashierEventDto;
        }


        public async Task<List<CashierEventBriefDto>> PrepareCashierEventBriefDtosAsync(List<ProductEvent> productEvents)
        {
            return await productEvents.SelectAwait(async pe =>
            {
                var balance = await _cashierBalanceService.GetBalanceAsync(pe.CashierEvent.Id);
                var eventDto = pe.EventDetail.Map<CashierEventBriefDto>();
                eventDto.Id = pe.Product.Id;
                eventDto.Name = await _localizationService.GetLocalizedAsync(pe.Product, p => p.Name);
                eventDto.Description = await _localizationService.GetLocalizedAsync(pe.Product, p => p.ShortDescription);
                eventDto.PictureModel = await PrepareEventPictureAsync(pe.Product);
                eventDto.EventStatusId = CalculateEventStatusSituation(pe.EventDetail.StartDate + pe.EventDetail.StartTime,
                    pe.EventDetail.EndDate + pe.EventDetail.EndTime);
                eventDto.OpeningFundBalanceAmount = balance;
                eventDto.OpeningFundBalance = await _priceFormatter.FormatPriceAsync(balance);

                return eventDto;
            }).ToListAsync();
        }


        public async Task<List<CategorizedEventBriefDto>> PrepareCategorizedEventBriefDtosAsync(
            List<CategorizedEventDetails> categorizedEvents)
        {
            if (categorizedEvents == null)
                return new List<CategorizedEventBriefDto>();


            return await categorizedEvents
                .GroupBy(pe => pe.Category.Id)
                .SelectAwait(async g => new CategorizedEventBriefDto
                {
                    CategoryId = g.Key,
                    CategoryName = await _localizationService.GetLocalizedAsync(g.ToList().First().Category, c => c.Name),
                    Events = await g
                        .SelectAwait(async x => await PrepareEventBriefDtoAsync(x.Product, x.EventDetail))
                        .ToListAsync(),
                })
                .ToListAsync();
        }
    }
}
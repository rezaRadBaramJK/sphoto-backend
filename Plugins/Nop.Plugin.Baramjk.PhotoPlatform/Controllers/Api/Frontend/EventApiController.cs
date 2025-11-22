using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Common;
using Nop.Services.Directory;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class EventApiController : BaseBaramjkApiController
    {
        private readonly EventService _eventService;
        private readonly EventFactory _eventFactory;
        private readonly IWorkContext _workContext;
        private readonly ISearchProductService _searchProductService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;

        public EventApiController(EventService eventService,
            EventFactory eventFactory,
            IWorkContext workContext,
            ISearchProductService searchProductService,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService)
        {
            _eventService = eventService;
            _eventFactory = eventFactory;
            _workContext = workContext;
            _searchProductService = searchProductService;
            _genericAttributeService = genericAttributeService;
            _countryService = countryService;
        }

        [HttpGet("/FrontendApi/PhotoPlatform/Event/{id:int}")]
        public async Task<IActionResult> GetEventByIdAsync(int id)
        {
            if (id < 1)
            {
                return ApiResponseFactory.BadRequest("Provided id is invalid");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            var countryId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute);

            if (countryId == 0)
            {
                var defaultCountry = await _countryService.GetCountryByTwoLetterIsoCodeAsync(DefaultValues.DefaultCountryTwoLetterIsoCode);
                countryId = defaultCountry.Id;
            }

            var eventDetails = await _eventService.GetEventFullDetailsAsync(id, countryId: countryId);
            if (eventDetails == null)
            {
                return ApiResponseFactory.BadRequest("Event not found");
            }

            var result = await _eventFactory.PrepareEventDtoAsync(eventDetails, false, true);

            return result == null ? ApiResponseFactory.BadRequest("Event not found") : ApiResponseFactory.Success(result);
        }

        [HttpGet("/FrontendApi/PhotoPlatform/Event/TimeSlot/{id:int}/Actors")]
        public async Task<IActionResult> GetTimeSlotActors(int id)
        {
            if (id < 1)
            {
                return ApiResponseFactory.BadRequest("Provided id is invalid");
            }

            var actors = await _eventService.GetTimeSlotActors(id);
            if (actors == null)
            {
                return ApiResponseFactory.BadRequest("Actor data not found");
            }

            var result = await _eventFactory.PrepareActorEventDtosAsync(actors, true);

            return result == null ? ApiResponseFactory.BadRequest("Actor data not found") : ApiResponseFactory.Success(result);
        }

        [HttpGet("/FrontendApi/PhotoPlatform/Event/")]
        public async Task<IActionResult> GetEventsAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var countryId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute);

            if (countryId == 0)
            {
                var defaultCountry = await _countryService.GetCountryByTwoLetterIsoCodeAsync(DefaultValues.DefaultCountryTwoLetterIsoCode);
                countryId = defaultCountry.Id;
            }

            var events = await _eventService.GetEventsAsync(countryId: countryId);
            var result = await _eventFactory.PrepareEventsAsync(events);


            return ApiResponseFactory.Success(result);
        }


        [HttpGet("/FrontendApi/PhotoPlatform/Event/Upcoming")]
        public async Task<IActionResult> GetUpcomingEventsAsync()
        {
            var events = await _eventService.GetEventsAsync(true);
            var result = await _eventFactory.PrepareEventsAsync(events);
            return ApiResponseFactory.Success(result);
        }


        [HttpGet("/FrontendApi/PhotoPlatform/Event/Bookmarked/")]
        public async Task<IActionResult> GetWishlistAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var events = await _eventService.GetCustomerBookMarkedEventsAsync(customer.Id);
            var result = await _eventFactory.PrepareBookMarkedEventsAsync(events);
            return ApiResponseFactory.Success(result);
        }


        [HttpGet("/FrontendApi/PhotoPlatform/Event/Categorized")]
        public async Task<IActionResult> GetCategorizedEventsAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            if (pageNumber < 1 || pageSize < 1)
                return ApiResponseFactory.BadRequest("Invalid page number or page size");


            var customer = await _workContext.GetCurrentCustomerAsync();
            var countryId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute);

            if (countryId == 0)
            {
                var defaultCountry = await _countryService.GetCountryByTwoLetterIsoCodeAsync(DefaultValues.DefaultCountryTwoLetterIsoCode);
                countryId = defaultCountry.Id;
            }

            var events = await _eventService.GetCategoriesEventsAsync(pageNumber - 1, pageSize, countryId);
            var result = await _eventFactory.PrepareCategorizedEventBriefDtosAsync(events);
            return ApiResponseFactory.Success(result);
        }

        [HttpGet("/FrontendApi/PhotoPlatform/Event/Search")]
        public async Task<IActionResult> SearchEventsAsync([FromQuery] string searchTerm, [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return ApiResponseFactory.BadRequest("Search term is required");

            if (pageNumber < 1 || pageSize < 1)
                return ApiResponseFactory.BadRequest("Invalid page number or page size");

            var products = await _searchProductService.SearchProductsAsync(keywords: searchTerm, pageIndex: pageNumber - 1, pageSize: pageSize);

            var eventsDetails = await _eventService.GetEventsBriefDataAsync(products.Select(p => p.Id).ToArray());

            var result = await _eventFactory.PrepareEventsPagedListAsync(eventsDetails, pageIndex: products.PageIndex, pageSize: products.PageSize,
                products.TotalCount);

            return ApiResponseFactory.Success(result);
        }
    }
}
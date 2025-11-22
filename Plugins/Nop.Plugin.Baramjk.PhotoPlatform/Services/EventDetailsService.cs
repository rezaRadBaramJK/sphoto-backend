using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.EventDetails;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class EventDetailsService
    {
        private readonly IRepository<EventDetail> _eventDetailsRepository;
        private readonly TimeSlotService _timeSlotService;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<EventCountryMapping> _eventCountryMappingRepository;
        private readonly IRepository<Country> _countryRepository;

        public EventDetailsService(
            IRepository<EventDetail> eventDetailsRepository,
            TimeSlotService timeSlotService,
            IRepository<Product> productRepository,
            IRepository<ActorEvent> actorEventRepository,
            IRepository<EventCountryMapping> eventCountryMappingRepository,
            IRepository<Country> countryRepository)
        {
            _eventDetailsRepository = eventDetailsRepository;
            _timeSlotService = timeSlotService;
            _productRepository = productRepository;
            _actorEventRepository = actorEventRepository;
            _eventCountryMappingRepository = eventCountryMappingRepository;
            _countryRepository = countryRepository;
        }

        public Task<EventDetail> GetByEventIdAsync(int eventId)
        {
            return _eventDetailsRepository.Table.FirstOrDefaultAsync(ed => ed.EventId == eventId);
        }


        public async Task SubmitAsync(EventDetail eventDetail)
        {
            var databaseEventDetail = await GetByEventIdAsync(eventDetail.EventId);
            if (databaseEventDetail == null)
            {
                await InsertAsync(eventDetail);
                return;
            }

            var actorEvents = await _actorEventRepository.Table
                .Where(ae => ae.EventId == eventDetail.EventId && ae.Deleted == false)
                .ToListAsync();

            if (actorEvents.Any())
            {
                actorEvents.ForEach(actorEvent =>
                {
                    actorEvent.CameraManEachPictureCost = eventDetail.PhotoPrice;
                    actorEvent.CustomerMobileEachPictureCost = eventDetail.PhotoPrice;
                    actorEvent.CommissionAmount = eventDetail.ActorShare;
                });


                await _actorEventRepository.UpdateAsync(actorEvents);
            }


            await UpdateAsync(eventDetail);
        }

        public Task InsertAsync(EventDetail eventDetail)
        {
            return Task.WhenAll(
                _eventDetailsRepository.InsertAsync(eventDetail),
                _timeSlotService.CreateTimeSlotsAsync(eventDetail));
        }

        public Task UpdateAsync(EventDetail eventDetail)
        {
            return _eventDetailsRepository.UpdateAsync(eventDetail);
        }

        public async Task<ProductEventDetails> GetProductEventDetailByProductIdAsync(int productId)
        {
            var query =
                from product in _productRepository.Table
                where product.Id == productId && product.Deleted == false
                join detail in _eventDetailsRepository.Table on product.Id equals detail.EventId into allDetails
                from detail in allDetails.DefaultIfEmpty()
                select new ProductEventDetails
                {
                    Product = product,
                    EventDetails = detail
                };

            var result = await query.FirstOrDefaultAsync();
            result ??= new ProductEventDetails();
            return result;
        }

        public Task<List<Country>> GetEventCountriesAsync(int eventId)
        {
            var q =
                from eventCountryMapping in _eventCountryMappingRepository.Table
                where eventCountryMapping.EventId == eventId
                join country in _countryRepository.Table on eventCountryMapping.CountryId equals country.Id
                where country.Published
                select country;

            return q.ToListAsync();
        }

        public async Task SubmitCountryMappingsAsync(int eventId, List<int> countryIds)
        {
            var prevMappings = _eventCountryMappingRepository.Table.Where(m => m.EventId == eventId).ToList();
            if (prevMappings.Any())
                await _eventCountryMappingRepository.DeleteAsync(prevMappings);


            var newMappings = countryIds
                .Select(countryId => new EventCountryMapping { CountryId = countryId, EventId = eventId, })
                .ToList();

            await _eventCountryMappingRepository.InsertAsync(newMappings);
        }

        public async Task DeleteEventCountryMappings(int eventId)
        {
            var prevMappings = _eventCountryMappingRepository.Table.Where(m => m.EventId == eventId).ToList();
            if (prevMappings.Any())
                await _eventCountryMappingRepository.DeleteAsync(prevMappings);
        }
    }
}
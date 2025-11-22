using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class ProductionEventService
    {
        private readonly IRepository<ProductionEvent> _productionEventRepository;
        private readonly IRepository<EventDetail> _eventDetailRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly IRepository<Actor> _actorRepository;

        public ProductionEventService(IRepository<ProductionEvent> productionEventRepository,
            IRepository<EventDetail> eventDetailRepository,
            IRepository<Product> productRepository,
            IRepository<ActorEvent> actorEventRepository,
            IRepository<TimeSlot> timeSlotRepository,
            IRepository<Actor> actorRepository)
        {
            _productionEventRepository = productionEventRepository;
            _eventDetailRepository = eventDetailRepository;
            _productRepository = productRepository;
            _actorEventRepository = actorEventRepository;
            _timeSlotRepository = timeSlotRepository;
            _actorRepository = actorRepository;
        }

        public Task<List<ProductionEvent>> GetEventProductionsAsync(int eventId, List<int> productionIds)
        {
            return _productionEventRepository.Table
                .Where(ce => ce.EventId == eventId && productionIds.Contains(ce.CustomerId) && ce.Deleted == false)
                .ToListAsync();
        }


        public Task<List<ProductionEvent>> GetEventProductionsAsync(int eventId)
        {
            return _productionEventRepository.Table
                .Where(ce => ce.EventId == eventId && ce.Deleted == false)
                .ToListAsync();
        }


        public Task InsertAsync(List<ProductionEvent> productionEvents)
        {
            return _productionEventRepository.InsertAsync(productionEvents);
        }


        public Task<IPagedList<ProductionEvent>> GetAllEventProductionEventsAsync(int eventId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return _productionEventRepository.Table
                .Where(ce => ce.EventId == eventId && ce.Deleted == false)
                .ToPagedListAsync(pageIndex, pageSize);
        }


        public Task<ProductionEvent> GetByIdAsync(int productionEventId)
        {
            return _productionEventRepository.GetByIdAsync(productionEventId, includeDeleted: false);
        }

        public Task<ProductionEvent> GetByProductionIdAsync(int productionId)
        {
            return _productionEventRepository.Table.FirstOrDefaultAsync(ce => ce.CustomerId == productionId && ce.Deleted == false);
        }


        public Task<ProductionEvent> GetByProductionIdAndEventIdAsync(int productionId, int eventId)
        {
            return _productionEventRepository.Table.FirstOrDefaultAsync(ce =>
                ce.CustomerId == productionId && ce.EventId == eventId && ce.Deleted == false);
        }

        public Task DeleteAsync(ProductionEvent productionEvent)
        {
            return _productionEventRepository.DeleteAsync(productionEvent);
        }


        public Task UpdateAsync(ProductionEvent productionEvent)
        {
            return _productionEventRepository.UpdateAsync(productionEvent);
        }

        public Task UpdateAsync(List<ProductionEvent> productionEvents)
        {
            return _productionEventRepository.UpdateAsync(productionEvents);
        }

        public Task<List<EventFullDetails>> GetProductionEventsFullDetailsAsync(int customerId, int[] eventIds = null)
        {
            var eventDetailsQuery =
                from productionEvent in _productionEventRepository.Table.Where(pe => pe.CustomerId == customerId && pe.Deleted == false)
                join eventDetail in _eventDetailRepository.Table on productionEvent.EventId equals eventDetail.EventId
                join product in _productRepository.Table on eventDetail.EventId equals product.Id
                where eventIds == null || eventIds.Contains(eventDetail.EventId)
                join timeSlot in
                    _timeSlotRepository.Table.Where(ts => ts.Deleted == false)
                    on
                    eventDetail.EventId equals timeSlot.EventId
                    into timeSlotGroup
                join actorEvent in _actorEventRepository.Table.Where(ae => ae.Deleted == false) on eventDetail.EventId
                    equals actorEvent.EventId into actorEventGroup
                select new EventFullDetails
                {
                    Product = product,
                    EventDetail = eventDetail,
                    TimeSlots = timeSlotGroup.Where(ts => ts.Deleted == false).ToList(),
                    Actors = actorEventGroup.OrderBy(ae => ae.DisplayOrder).Select(ae => new EventDetailActor
                    {
                        Actor = _actorRepository.Table.FirstOrDefault(a => a.Id == ae.ActorId),
                        CameraManEachPictureCost = ae.CameraManEachPictureCost,
                        CustomerMobileEachPictureCost = ae.CustomerMobileEachPictureCost,
                    })
                };
            return eventDetailsQuery.ToListAsync();
        }

        public Task<List<ProductionEvent>> GetEventsProductionRoles(int[] eventIds)
        {
            return _productionEventRepository.Table.Where(x => eventIds.Contains(x.EventId) && x.Deleted == false && x.Active == true).ToListAsync();
        }
    }
}
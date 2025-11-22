using System;
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
    public class SupervisorEventService
    {
        private readonly IRepository<SupervisorEvent> _supervisorEventRepository;
        private readonly IRepository<EventDetail> _eventDetailRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<Actor> _actorRepository;

        public SupervisorEventService(IRepository<SupervisorEvent> supervisorEventRepository,
            IRepository<EventDetail> eventDetailRepository,
            IRepository<Product> productRepository,
            IRepository<TimeSlot> timeSlotRepository,
            IRepository<ActorEvent> actorEventRepository,
            IRepository<Actor> actorRepository)
        {
            _supervisorEventRepository = supervisorEventRepository;
            _eventDetailRepository = eventDetailRepository;
            _productRepository = productRepository;
            _timeSlotRepository = timeSlotRepository;
            _actorEventRepository = actorEventRepository;
            _actorRepository = actorRepository;
        }

        public Task<List<SupervisorEvent>> GetEventSupervisorsAsync(int eventId, List<int> supervisorIds)
        {
            return _supervisorEventRepository.Table
                .Where(ce => ce.EventId == eventId && supervisorIds.Contains(ce.CustomerId) && ce.Deleted == false)
                .ToListAsync();
        }


        public Task<List<SupervisorEvent>> GetEventSupervisorsAsync(int eventId)
        {
            return _supervisorEventRepository.Table
                .Where(ce => ce.EventId == eventId && ce.Deleted == false)
                .ToListAsync();
        }


        public Task InsertAsync(List<SupervisorEvent> supervisorEvents)
        {
            return _supervisorEventRepository.InsertAsync(supervisorEvents);
        }


        public Task<IPagedList<SupervisorEvent>> GetAllEventSupervisorEventsAsync(int eventId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return _supervisorEventRepository.Table
                .Where(ce => ce.EventId == eventId && ce.Deleted == false)
                .ToPagedListAsync(pageIndex, pageSize);
        }


        public Task<SupervisorEvent> GetByIdAsync(int supervisorEventId)
        {
            return _supervisorEventRepository.GetByIdAsync(supervisorEventId, includeDeleted: false);
        }

        public Task<SupervisorEvent> GetBySupervisorIdAsync(int supervisorId)
        {
            return _supervisorEventRepository.Table.FirstOrDefaultAsync(ce => ce.CustomerId == supervisorId && ce.Deleted == false);
        }

        public Task<SupervisorEvent> GetByEventIdAsync(int eventId)
        {
            return _supervisorEventRepository.Table.FirstOrDefaultAsync(ce => ce.EventId == eventId && ce.Deleted == false);
        }


        public Task<SupervisorEvent> GetBySupervisorIdAndEventIdAsync(int supervisorId, int eventId)
        {
            return _supervisorEventRepository.Table.FirstOrDefaultAsync(ce =>
                ce.CustomerId == supervisorId && ce.EventId == eventId && ce.Deleted == false);
        }

        public Task DeleteAsync(SupervisorEvent supervisorEvent)
        {
            return _supervisorEventRepository.DeleteAsync(supervisorEvent);
        }


        public Task UpdateAsync(SupervisorEvent supervisorEvent)
        {
            return _supervisorEventRepository.UpdateAsync(supervisorEvent);
        }

        public Task UpdateAsync(List<SupervisorEvent> supervisorEvents)
        {
            return _supervisorEventRepository.UpdateAsync(supervisorEvents);
        }

        public Task<List<ProductEvent>> GetSupervisorEventsAsync(int supervisorId)
        {
            var query =
                from se in _supervisorEventRepository.Table
                where se.CustomerId == supervisorId && se.Deleted == false
                join eventDetail in _eventDetailRepository.Table on se.EventId equals eventDetail.EventId
                join product in _productRepository.Table
                    on eventDetail.EventId equals product.Id
                where eventDetail.EndDate >= DateTime.Today &&
                      product.Published &&
                      product.Deleted == false
                select new ProductEvent
                {
                    Product = product,
                    EventDetail = eventDetail,
                };
            return query.ToListAsync();
        }

        public Task<EventFullDetails> GetSupervisorEventFullDetailsAsync(int eventId, int supervisorId,
            bool includeDeactivatedTimeSlots = false)
        {
            var eventDetailsQuery =
                from se in _supervisorEventRepository.Table
                where se.CustomerId == supervisorId && se.Deleted == false
                join eventDetail in _eventDetailRepository.Table on se.EventId equals eventDetail.EventId
                join product in _productRepository.Table on eventDetail.EventId equals product.Id
                where eventDetail.EventId == eventId
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
                    TimeSlots = timeSlotGroup.Where(ts => (includeDeactivatedTimeSlots || ts.Active) && ts.Deleted == false).ToList(),
                    Actors = actorEventGroup.OrderBy(ae => ae.DisplayOrder).Select(ae => new EventDetailActor
                    {
                        Actor = _actorRepository.Table.FirstOrDefault(a => a.Id == ae.ActorId),
                        CameraManEachPictureCost = ae.CameraManEachPictureCost,
                        CustomerMobileEachPictureCost = ae.CustomerMobileEachPictureCost,
                    })
                };


            return eventDetailsQuery.FirstOrDefaultAsync();
        }

        public Task<List<EventFullDetails>> GetEventsFullDetailsAsync(int supervisorId, int[] eventIds = null)
        {
            var eventDetailsQuery =
                from se in _supervisorEventRepository.Table
                where se.CustomerId == supervisorId && se.Deleted == false
                join eventDetail in _eventDetailRepository.Table on se.EventId equals eventDetail.EventId
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
    }
}
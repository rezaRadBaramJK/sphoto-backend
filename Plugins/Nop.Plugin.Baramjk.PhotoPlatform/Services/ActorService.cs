using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Actors;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Actors;
using Nop.Services.Common;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class ActorService
    {
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<ActorPicture> _actorPictureRepository;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly IRepository<EventDetail> _eventDetailRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ReservationItem> _reservationItemRepository;
        private readonly IRepository<Order> _orderRepository;


        public ActorService(IRepository<Actor> actorRepository,
            IRepository<Customer> customerRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            IGenericAttributeService genericAttributeService,
            IRepository<ActorPicture> actorPictureRepository,
            IRepository<Picture> pictureRepository,
            IRepository<ActorEvent> actorEventRepository,
            IRepository<TimeSlot> timeSlotRepository,
            IRepository<EventDetail> eventDetailRepository,
            IRepository<Product> productRepository,
            IRepository<ReservationItem> reservationItemRepository,
            IRepository<Order> orderRepository)
        {
            _actorRepository = actorRepository;
            _customerRepository = customerRepository;
            _genericAttributeRepository = genericAttributeRepository;
            _genericAttributeService = genericAttributeService;
            _actorPictureRepository = actorPictureRepository;
            _pictureRepository = pictureRepository;
            _actorEventRepository = actorEventRepository;
            _timeSlotRepository = timeSlotRepository;
            _eventDetailRepository = eventDetailRepository;
            _productRepository = productRepository;
            _reservationItemRepository = reservationItemRepository;
            _orderRepository = orderRepository;
        }

        public Task<IPagedList<Actor>> GetAllAsync(string email = null, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var q = from actor in _actorRepository.Table
                join customer in _customerRepository.Table on actor.CustomerId equals customer.Id
                where actor.Deleted == false
                select new { actor, customer };

            if (string.IsNullOrEmpty(email) == false)
            {
                q = q.Where(p => p.customer.Email.Contains(email));
            }

            if (string.IsNullOrEmpty(name) == false)
            {
                q = q.Where(p => p.actor.Name.Contains(name));
            }


            return q.Select(p => p.actor).ToPagedListAsync(pageIndex, pageSize);
        }


        public Task<IPagedList<Actor>> GetNotAssociatedActorsAsync(
            int eventId,
            string email = null,
            string name = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var q = from actor in _actorRepository.Table
                join custGroup in _customerRepository.Table
                    on actor.CustomerId equals custGroup.Id into custG
                from customer in custG.DefaultIfEmpty()
                where actor.Deleted == false
                      && _actorEventRepository.Table.Any(ae =>
                          ae.ActorId == actor.Id
                          && ae.EventId == eventId
                          && ae.Deleted == false
                      ) == false
                select new { actor, customer };

            if (!string.IsNullOrEmpty(email))
            {
                q = q.Where(p => p.customer.Email.Contains(email));
            }

            if (!string.IsNullOrEmpty(name))
            {
                q = q.Where(p => p.actor.Name.Contains(name));
            }

            return q.Select(p => p.actor).Distinct().ToPagedListAsync(pageIndex, pageSize);
        }

        public Task<Actor> GetByIdAsync(int id)
        {
            return _actorRepository.GetByIdAsync(id, includeDeleted: false);
        }

        public Task<IList<Actor>> GetByIdsAsync(int[] ids)
        {
            return _actorRepository.GetByIdsAsync(ids, includeDeleted: false);
        }

        public Task InsertAsync(Actor actor)
        {
            return _actorRepository.InsertAsync(actor);
        }

        public Task UpdateAsync(Actor actor)
        {
            return _actorRepository.UpdateAsync(actor);
        }

        public Task DeleteAsync(Actor actor)
        {
            return _actorRepository.DeleteAsync(actor);
        }

        public Task<Actor> GetByCustomerIdAsync(int customerId)
        {
            return _actorRepository.Table.Where(a => a.CustomerId == customerId && a.Deleted == false).FirstOrDefaultAsync();
        }

        public async Task UpdateActorInfoAsync(UpdateActorInfoApiParams apiParams, Customer customer, Actor actor)
        {
            if (string.IsNullOrEmpty(apiParams.Email) == false)
            {
                customer.Email = apiParams.Email;

                await _customerRepository.UpdateAsync(customer);
            }

            if (string.IsNullOrEmpty(apiParams.Name) == false)
            {
                actor.Name = apiParams.Name;
            }

            if (string.IsNullOrEmpty(apiParams.PhoneNumber) == false)
            {
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PhoneAttribute, apiParams.PhoneNumber);
            }


            if (string.IsNullOrEmpty(apiParams.CardNumber) == false)
            {
                actor.CardNumber = apiParams.CardNumber;
            }

            if (string.IsNullOrEmpty(apiParams.CardHolderName) == false)
            {
                actor.CardHolderName = apiParams.CardHolderName;
            }

            await _actorRepository.UpdateAsync(actor);
            await _customerRepository.UpdateAsync(customer);
        }

        public async Task<ActorInfoDto> GetActorDetailsAsync(int customerId)
        {
            var query =
                from actor in _actorRepository.Table
                join customer in _customerRepository.Table on actor.CustomerId equals customer.Id
                where actor.CustomerId == customerId && actor.Deleted == false
                join ga in _genericAttributeRepository.Table
                    on new { EntityId = customer.Id, KeyGroup = nameof(Customer), Key = NopCustomerDefaults.PhoneAttribute }
                    equals new { ga.EntityId, ga.KeyGroup, ga.Key } into phoneAttrs
                from phone in phoneAttrs.DefaultIfEmpty()
                select new ActorInfoDto
                {
                    Id = actor.Id,
                    Email = customer.Email,
                    PhoneNumber = phone != null ? phone.Value : null,
                    Name = actor.Name,
                };
            return await query.FirstOrDefaultAsync();
        }


        public Task<IPagedList<ActorPicture>> GetActorPicturesAsync(int actorId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return _actorPictureRepository.Table.Where(ap => ap.ActorId == actorId).ToPagedListAsync(pageIndex, pageSize);
        }

        public Task<ActorPicture> GetActorPictureByIdAsync(int id)
        {
            return _actorPictureRepository.GetByIdAsync(id);
        }

        public Task DeleteActorPictureAsync(ActorPicture actorPicture)
        {
            return _actorPictureRepository.DeleteAsync(actorPicture);
        }

        public Task UpdateActorPictureAsync(ActorPicture actorPicture)
        {
            return _actorPictureRepository.UpdateAsync(actorPicture);
        }

        public Task<List<ActorPicture>> GetActorPicturesByActorIdAsync(int actorId)
        {
            return _actorPictureRepository.Table.Where(ap => ap.ActorId == actorId).OrderBy(ap => ap.DisplayOrder).ToListAsync();
        }

        public async Task<List<Picture>> GetPicturesByActorIdAsync(int actorId, int recordsToReturn = 0)
        {
            if (actorId == 0)
                return new List<Picture>();

            var query =
                from picture in _pictureRepository.Table
                join actorPicture in _actorPictureRepository.Table on picture.Id equals actorPicture.PictureId
                orderby actorPicture.DisplayOrder, actorPicture.Id
                where actorPicture.ActorId == actorId
                select picture;

            if (recordsToReturn > 0) query = query.Take(recordsToReturn);

            return await query.ToListAsync();
        }


        public Task InsertActorPictureAsync(ActorPicture actorPicture)
        {
            return _actorPictureRepository.InsertAsync(actorPicture);
        }

        public Task<Actor> GetByEmailAsync(string email)
        {
            var q = from actor in _actorRepository.Table
                join customer in _customerRepository.Table on actor.CustomerId equals customer.Id
                where customer.Email == email && actor.Deleted == false
                select actor;
            return q.FirstOrDefaultAsync();
        }

        public Task<List<ActorReportDetailsModel>> GetActorDetailedEventsAsync(int actorId = 0, int customerId = 0)
        {
            var query =
                from actorEvent in _actorEventRepository.Table
                join actor in _actorRepository.Table on actorEvent.ActorId equals actor.Id
                join product in _productRepository.Table on actorEvent.EventId equals product.Id
                join eventDetail in _eventDetailRepository.Table on actorEvent.EventId equals eventDetail.EventId
                join timeSlot in _timeSlotRepository.Table on eventDetail.EventId equals timeSlot.EventId into timeSlotsGroup
                where (actorId == 0 || actorEvent.ActorId == actorId)
                      && (customerId == 0 || actor.CustomerId == customerId)
                      && actorEvent.Deleted == false
                select new ActorReportDetailsModel
                {
                    Product = product,
                    EventDetail = eventDetail,
                    TimeSlots = timeSlotsGroup.ToList()
                };

            return query.ToListAsync();
        }


        public Task<List<ActorRevenueReportDetailsModel>> GetActorReportData(int customerId, int eventId, DateTime? date, int? timeSlotId = 0)
        {
            var query =
                from reservationItem in _reservationItemRepository.Table
                join actor in _actorRepository.Table on reservationItem.ActorId equals actor.Id
                join order in _orderRepository.Table on reservationItem.OrderId equals order.Id
                join timeSlot in _timeSlotRepository.Table on reservationItem.TimeSlotId equals timeSlot.Id
                join actorEvent in _actorEventRepository.Table
                    on new { reservationItem.EventId, ActorId = actor.Id }
                    equals new { actorEvent.EventId, actorEvent.ActorId }
                where reservationItem.EventId == eventId
                      && actor.CustomerId == customerId
                      && (date == null || timeSlot.Date.Date == date)
                      && (timeSlotId == null || timeSlot.Id == timeSlotId)
                group new { reservationItem, order, actorEvent, timeSlot }
                    by reservationItem.OrderId
                into g
                select new ActorRevenueReportDetailsModel
                {
                    Order = g.Select(x => x.order).FirstOrDefault(),
                    ReservationItems = g.Select(x => x.reservationItem).ToList(),
                    ActorEvent = g.Select(x => x.actorEvent).FirstOrDefault(),
                    TimeSlot = g.Select(x => x.timeSlot).FirstOrDefault()
                };


            return query.ToListAsync();
        }

        public Task<List<ActorTimeSlotRevenueReportDetailsModel>> GetActorRevenueDataAsync(int customerId, int eventId, DateTime? date,
            int? timeSlotId = 0)
        {
            var query =
                from reservationItem in _reservationItemRepository.Table
                join actor in _actorRepository.Table on reservationItem.ActorId equals actor.Id
                join timeSlot in _timeSlotRepository.Table on reservationItem.TimeSlotId equals timeSlot.Id
                join eventDetail in _eventDetailRepository.Table on reservationItem.EventId equals eventDetail.EventId
                join product in _productRepository.Table on reservationItem.EventId equals product.Id
                join actorEvent in _actorEventRepository.Table
                    on new { reservationItem.EventId, ActorId = actor.Id }
                    equals new { actorEvent.EventId, actorEvent.ActorId }
                where reservationItem.EventId == eventId
                      && actor.CustomerId == customerId
                      && (date == null || timeSlot.Date.Date == date)
                      && (timeSlotId == null || timeSlot.Id == timeSlotId)
                select new ActorTimeSlotRevenueReportDetailsModel
                {
                    ReservationItem = reservationItem,
                    ActorEvent = actorEvent,
                    TimeSlot = timeSlot,
                    EventDetail = eventDetail,
                    Product = product,
                };


            return query.ToListAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class EventService
    {
        private readonly IRepository<EventDetail> _eventDetailRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ReservationItem> _reservationItemRepository;
        private readonly IRepository<CashierEvent> _cashierEventRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<EventCountryMapping> _eventCountryMappingRepository;
        private readonly IRepository<ActorEventTimeSlot> _actorEventTimeSlotRepository;


        public EventService(
            IRepository<EventDetail> eventDetailRepository,
            IRepository<Product> productRepository,
            IRepository<ReservationItem> reservationItemRepository,
            IRepository<CashierEvent> cashierEventRepository,
            IRepository<TimeSlot> timeSlotRepository,
            IRepository<ActorEvent> actorEventRepository,
            IRepository<Actor> actorRepository,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IRepository<Category> categoryRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<Customer> customerRepository,
            IRepository<EventCountryMapping> eventCountryMappingRepository,
            IRepository<ActorEventTimeSlot> actorEventTimeSlotRepository)
        {
            _eventDetailRepository = eventDetailRepository;
            _productRepository = productRepository;
            _reservationItemRepository = reservationItemRepository;
            _cashierEventRepository = cashierEventRepository;
            _timeSlotRepository = timeSlotRepository;
            _actorEventRepository = actorEventRepository;
            _actorRepository = actorRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _categoryRepository = categoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _customerRepository = customerRepository;
            _eventCountryMappingRepository = eventCountryMappingRepository;
            _actorEventTimeSlotRepository = actorEventTimeSlotRepository;
        }

        public Task<EventDetail> GetEventDetailByEvent(Product productEvent)
        {
            return _eventDetailRepository.Table.Where(ed => ed.EventId == productEvent.Id).FirstOrDefaultAsync();
        }

        public Task<EventDetail> GetEventDetailByEventId(int eventId)
        {
            return _eventDetailRepository.Table.Where(ed => ed.EventId == eventId).FirstOrDefaultAsync();
        }

        public Task<List<EventDetail>> GetEventDetailByEventIds(List<int> eventIds)
        {
            return _eventDetailRepository.Table.Where(ed => eventIds.Contains(ed.EventId)).ToListAsync();
        }

        public Task<List<Customer>> GetEventCashiersDetails(int eventId)
        {
            var query =
                from cashierEvent in _cashierEventRepository.Table
                where cashierEvent.EventId == eventId && cashierEvent.Deleted == false
                join customer in _customerRepository.Table on cashierEvent.CustomerId equals customer.Id
                select customer;

            return query.ToListAsync();
        }

        public Task<List<EventDetailActor>> GetTimeSlotActors(int timeSlotId)
        {
            var query =
                from ts in _timeSlotRepository.Table
                where ts.Id == timeSlotId
                      && ts.Deleted == false
                      && (ts.Active == true)
                join ae in _actorEventRepository.Table
                        .Where(ae => ae.Deleted == false)
                    on ts.EventId equals ae.EventId
                join aetsKey in _actorEventTimeSlotRepository.Table
                    on new { ActorEventId = ae.Id, TimeSlotId = ts.Id }
                    equals new { aetsKey.ActorEventId, aetsKey.TimeSlotId }
                    into aetsGroup
                from aets in aetsGroup.DefaultIfEmpty()
                where (aets == null) || (aets.IsDeactivated != true)
                join actor in _actorRepository.Table
                        .Where(a => a.Deleted == false)
                    on ae.ActorId equals actor.Id
                select new EventDetailActor
                {
                    CameraManEachPictureCost = ae.CameraManEachPictureCost,
                    CustomerMobileEachPictureCost = ae.CustomerMobileEachPictureCost,
                    Actor = actor
                };

            return query.ToListAsync();
        }

        public async Task<EventFullDetails> GetEventFullDetailsAsync(int eventId, int cashierId = 0, int countryId = 0,
            bool includeDeactivatedTimeSlots = false)
        {
            var eventDetailsQuery =
                from eventDetail in _eventDetailRepository.Table
                join product in _productRepository.Table on eventDetail.EventId equals product.Id
                where eventDetail.EventId == eventId
                join timeSlot in
                    _timeSlotRepository.Table.Where(ts => ts.Deleted == false)
                    on
                    eventDetail.EventId equals timeSlot.EventId
                    into timeSlotGroup
                join actorEvent in _actorEventRepository.Table.Where(ae => ae.Deleted == false) on eventDetail.EventId
                    equals actorEvent.EventId into actorEventGroup
                //! Please be careful about create instance, It is created in cashier query also
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

            if (countryId > 0)
            {
                var allowedEventIds = _eventCountryMappingRepository.Table
                    .Where(m => m.CountryId == countryId)
                    .Select(m => m.EventId)
                    .Distinct()
                    .ToList();

                eventDetailsQuery = eventDetailsQuery
                    .Where(e => allowedEventIds.Contains(e.EventDetail.EventId));
            }

            if (cashierId > 0)
            {
                var filterByCashierIdQuery =
                    from eventDetails in eventDetailsQuery
                    join cashierEvent in _cashierEventRepository.Table on eventDetails.EventDetail.EventId equals cashierEvent.EventId
                    where cashierEvent.CustomerId == cashierId && cashierEvent.Deleted == false && cashierEvent.Active
                    select new
                    {
                        eventDetails,
                        cashierEvent
                    };

                var result = await filterByCashierIdQuery.FirstOrDefaultAsync();
                if (result?.eventDetails == null)
                    return null;

                result.eventDetails.CashierEvent = result.cashierEvent;
                return result.eventDetails;
            }

            return await eventDetailsQuery.FirstOrDefaultAsync();
        }


        public Task<List<ProductEvent>> GetCashierEventsAsync(int customerId)
        {
            var query =
                from eventDetail in _eventDetailRepository.Table
                join product in _productRepository.Table
                    on eventDetail.EventId equals product.Id
                where
                    product.Published &&
                    product.Deleted == false
                join cashierEvent in _cashierEventRepository.Table on eventDetail.EventId equals cashierEvent.EventId
                where cashierEvent.CustomerId == customerId && cashierEvent.Active && cashierEvent.Deleted == false
                select new ProductEvent
                {
                    Product = product,
                    EventDetail = eventDetail,
                    CashierEvent = cashierEvent
                };
            return query.ToListAsync();
        }

        public async Task<List<ProductEvent>> GetEventsAsync(bool onlyUpcoming = false, int countryId = 0)
        {
            var query =
                from eventDetail in _eventDetailRepository.Table
                join product in _productRepository.Table
                    on eventDetail.EventId equals product.Id
                where eventDetail.EndDate >= DateTime.Today &&
                      (onlyUpcoming == false || eventDetail.StartDate >= DateTime.Now) &&
                      product.Published &&
                      product.Deleted == false
                join reservationItem in _reservationItemRepository.Table on eventDetail.EventId equals reservationItem
                        .EventId
                    into reservationGroup
                select new
                {
                    Product = product,
                    EventDetail = eventDetail,
                    ReservationCount = reservationGroup.Count()
                };

            if (countryId > 0)
            {
                var allowedEventIds = _eventCountryMappingRepository.Table
                    .Where(m => m.CountryId == countryId)
                    .Select(m => m.EventId)
                    .Distinct()
                    .ToList();

                query = query.Where(e => allowedEventIds.Contains(e.EventDetail.EventId));
            }

            var results = await query.ToListAsync();

            var topItem = results.OrderByDescending(item => item.ReservationCount).FirstOrDefault();

            if (topItem != null)
                results.Remove(topItem);

            var range = new Random();

            //shuffle results
            results = results.OrderBy(_ => range.Next()).ToList();

            if (topItem != null)
                results.Insert(0, topItem);

            return results.Select(r => new ProductEvent
            {
                Product = r.Product,
                EventDetail = r.EventDetail,
            }).ToList();
        }

        public Task<List<Product>> GetEventsBriefDataAsync()
        {
            var query =
                from eventDetail in _eventDetailRepository.Table
                join product in _productRepository.Table
                    on eventDetail.EventId equals product.Id
                where
                    product.Published &&
                    product.Deleted == false
                select product;
            return query.ToListAsync();
        }

        public Task<List<ProductEvent>> GetEventsBriefDataAsync(int[] eventIds)
        {
            var query =
                from eventDetail in _eventDetailRepository.Table
                join product in _productRepository.Table
                    on eventDetail.EventId equals product.Id
                where eventDetail.EndDate >= DateTime.Today &&
                      product.Published &&
                      product.Deleted == false &&
                      eventIds.Contains(product.Id)
                select new ProductEvent
                {
                    Product = product,
                    EventDetail = eventDetail
                };
            return query.ToListAsync();
        }

        public Task<List<BookmarkedEventModel>> GetCustomerBookMarkedEventsAsync(int customerId)
        {
            var query =
                from shoppingCartItem in _shoppingCartItemRepository.Table
                join product in _productRepository.Table on shoppingCartItem.ProductId equals product.Id
                join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                where shoppingCartItem.CustomerId == customerId &&
                      shoppingCartItem.ShoppingCartTypeId == (int)ShoppingCartType.Wishlist
                select new BookmarkedEventModel
                {
                    Product = product,
                    EventDetail = eventDetail,
                    ShoppingCartItem = shoppingCartItem,
                };

            return query.ToListAsync();
        }

        public async Task<List<CategorizedEventDetails>> GetCategoriesEventsAsync(int pageNumber, int pageSize, int countryId = 0)
        {
            List<int> allowedEventIds = new();
            if (countryId > 0)
            {
                allowedEventIds = await _eventCountryMappingRepository.Table
                    .Where(m => m.CountryId == countryId)
                    .Select(m => m.EventId)
                    .Distinct()
                    .ToListAsync();
            }

            var query =
                (from category in _categoryRepository.Table
                    where category.Published && !category.Deleted
                    orderby category.DisplayOrder
                    select new
                    {
                        Category = category,
                        Products = from productCategory in _productCategoryRepository.Table
                            join product in _productRepository.Table on productCategory.ProductId equals product.Id
                            join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                            where productCategory.CategoryId == category.Id
                                  && product.Published && !product.Deleted
                                  && (countryId == 0 || allowedEventIds.Contains(eventDetail.EventId))
                            orderby product.DisplayOrder
                            select new { Product = product, EventDetail = eventDetail }
                    })
                .Skip(pageNumber * pageSize)
                .Take(pageSize);

            var result = await query.ToListAsync();

            var categorizedEventDetails = result
                .SelectMany(c => c.Products
                    .Where(x => x.EventDetail.EndDate.Add(x.EventDetail.EndTime) >= DateTime.Now)
                    .Select(p =>
                        new CategorizedEventDetails
                        {
                            Category = c.Category,
                            Product = p.Product,
                            EventDetail = p.EventDetail
                        }))
                .ToList();

            return categorizedEventDetails;
        }
    }
}
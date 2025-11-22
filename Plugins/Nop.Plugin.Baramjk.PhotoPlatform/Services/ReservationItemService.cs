using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reservations;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reservations;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Services;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class ReservationItemService
    {
        private readonly IRepository<ReservationItem> _reservationItemRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<EventDetail> _eventDetailRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly EventFactory _eventFactory;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly ActorEventService _actorEventService;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IPushNotificationTokenService _pushNotificationTokenService;
        private readonly IPushNotificationSenderService _pushNotificationSenderService;
        private readonly ProductionEventService _productionEventService;
        private readonly ILocalizationService _localizationService;


        public ReservationItemService(IRepository<ReservationItem> reservationItemRepository,
            IRepository<Order> orderRepository,
            IRepository<Actor> actorRepository,
            IRepository<Product> productRepository,
            IRepository<EventDetail> eventDetailRepository,
            IRepository<Customer> customerRepository,
            EventFactory eventFactory,
            IPriceFormatter priceFormatter,
            IRepository<ActorEvent> actorEventRepository,
            IRepository<TimeSlot> timeSlotRepository,
            ActorEventService actorEventService,
            IRepository<GenericAttribute> genericAttributeRepository,
            IPushNotificationTokenService pushNotificationTokenService,
            IPushNotificationSenderService pushNotificationSenderService,
            ProductionEventService productionEventService,
            ILocalizationService localizationService)
        {
            _reservationItemRepository = reservationItemRepository;
            _orderRepository = orderRepository;
            _actorRepository = actorRepository;
            _productRepository = productRepository;
            _eventDetailRepository = eventDetailRepository;
            _customerRepository = customerRepository;
            _eventFactory = eventFactory;
            _priceFormatter = priceFormatter;
            _actorEventRepository = actorEventRepository;
            _timeSlotRepository = timeSlotRepository;
            _actorEventService = actorEventService;
            _genericAttributeRepository = genericAttributeRepository;
            _pushNotificationTokenService = pushNotificationTokenService;
            _pushNotificationSenderService = pushNotificationSenderService;
            _productionEventService = productionEventService;
            _localizationService = localizationService;
        }


        public Task UpdateAsync(ReservationItem reservationItem)
        {
            return _reservationItemRepository.UpdateAsync(reservationItem);
        }

        public Task DeleteAsync(ReservationItem reservationItem)
        {
            return _reservationItemRepository.DeleteAsync(reservationItem);
        }

        public Task UpdateAsync(List<ReservationItem> reservationItems)
        {
            return _reservationItemRepository.UpdateAsync(reservationItems);
        }


        public Task<ReservationItem> GetByIdAsync(int id)
        {
            return _reservationItemRepository.GetByIdAsync(id);
        }

        public Task<List<ReservationItem>> GetAllOrderReservationsAsync(int orderId)
        {
            return _reservationItemRepository.Table.Where(ri => ri.OrderId == orderId).ToListAsync();
        }


        public async Task ChangeReservationsStatusAsync(int orderId, int reservationStatusId)
        {
            var reservations = await _reservationItemRepository.Table
                .Where(ri => ri.OrderId == orderId)
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                reservation.ReservationStatusId = reservationStatusId;
            }

            await _reservationItemRepository.UpdateAsync(reservations);
        }


        public Task<List<ReservationDetailsModel>> GetReservationDetailsByOrderIdAsync(int orderId)
        {
            var query =
                from reservation in _reservationItemRepository.Table
                join actorEvent in _actorEventRepository.Table
                    on new { reservation.ActorId, reservation.EventId }
                    equals new { actorEvent.ActorId, actorEvent.EventId }
                where reservation.OrderId == orderId && actorEvent.Deleted == false
                join product in _productRepository.Table on reservation.EventId equals product.Id
                join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                join timeSlot in _timeSlotRepository.Table on reservation.TimeSlotId equals timeSlot.Id
                join actor in _actorRepository.Table on actorEvent.ActorId equals actor.Id
                select new ReservationDetailsModel()
                {
                    ReservationItem = reservation,
                    ActorEvent = actorEvent,
                    TimeSlot = timeSlot,
                    Product = product,
                    Actor = actor,
                    EventDetail = eventDetail
                };

            return query.ToListAsync();
        }

        public Task<IPagedList<OrderReservationDetailsModel>> GetCustomerOrdersReservationsAsync(int customerId, string phoneNumber = null,
            int eventId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query =
                from order in _orderRepository.Table
                where order.CustomerId == customerId && !order.Deleted
                join reservation in _reservationItemRepository.Table
                    on order.Id equals reservation.OrderId into reservationGroup
                from reservationItem in reservationGroup.DefaultIfEmpty()
                join actorEvent in _actorEventRepository.Table
                    on new { reservationItem.ActorId, reservationItem.EventId }
                    equals new { actorEvent.ActorId, actorEvent.EventId }
                    into actorEventGroup
                from actorEvent in actorEventGroup.DefaultIfEmpty()
                where actorEvent.Deleted == false
                join product in _productRepository.Table
                    on reservationItem.EventId equals product.Id
                join timeSlot in _timeSlotRepository.Table
                    on reservationItem.TimeSlotId equals timeSlot.Id into timeSlotGroup
                from timeSlot in timeSlotGroup.DefaultIfEmpty()
                join actor in _actorRepository.Table
                    on reservationItem.ActorId equals actor.Id into actorGroup
                from actor in actorGroup.DefaultIfEmpty()
                select new { order, reservationItem, actorEvent, product, timeSlot, actor };

            if (eventId > 0)
            {
                query = query.Where(x => x.product.Id == eventId);
            }

            var groupedQuery =
                from x in query
                group new ReservationDetailsModel
                    {
                        ReservationItem = x.reservationItem,
                        ActorEvent = x.actorEvent,
                        Product = x.product,
                        TimeSlot = x.timeSlot,
                        Actor = x.actor
                    }
                    by x.order
                into g
                select new OrderReservationDetailsModel
                {
                    Order = g.Key,
                    OrderReservationDetails = g
                        .Where(x => x.ReservationItem != null)
                        .ToList()
                };

            if (string.IsNullOrEmpty(phoneNumber) == false)
            {
                groupedQuery =
                    from g in groupedQuery
                    join ga in _genericAttributeRepository.Table
                        on g.Order.Id equals ga.EntityId
                    where ga.Key == DefaultValues.CustomerPhoneForCashierOrderAttributeKey
                          && ga.Value == phoneNumber
                    select g;
            }

            return groupedQuery.ToPagedListAsync(pageIndex, pageSize);
        }


        public async Task<List<ActorReservationsDto>> GetActorReservationsAsync(Customer actorCustomer)
        {
            var query =
                from reservation in _reservationItemRepository.Table
                join actor in _actorRepository.Table on reservation.ActorId equals actor.Id
                join customer in _customerRepository.Table on actor.CustomerId equals customer.Id
                join product in _productRepository.Table on reservation.EventId equals product.Id
                join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                join actorEvent in _actorEventRepository.Table
                    on new { reservation.ActorId, reservation.EventId }
                    equals new { actorEvent.ActorId, actorEvent.EventId }
                where actorEvent.Deleted == false && actor.CustomerId == actorCustomer.Id
                join timeSlot in _timeSlotRepository.Table on reservation.TimeSlotId equals timeSlot.Id
                select new
                {
                    reservation,
                    actor,
                    customer,
                    product,
                    eventDetail,
                    actorEvent,
                    timeSlot
                };

            var queryResult = await query.ToListAsync();


            return await queryResult
                .GroupBy(x => new
                {
                    x.reservation.EventId,
                }).ToList().SelectAwait(async g =>
                {
                    var firstItem = g.First();
                    return new ActorReservationsDto
                    {
                        EventId = g.Key.EventId,

                        Picture = await _eventFactory.PrepareEventPictureAsync(firstItem.product),
                        EventName = firstItem.product.Name,

                        TotalCommission = await _priceFormatter.FormatPriceAsync(
                            g.Sum(x => x.reservation.UsedCameraManPhotoCount + x.reservation.UsedCustomerMobilePhotoCount) *
                            firstItem.actorEvent.CommissionAmount),
                        CameramanPhotoCount = g.Sum(x => x.reservation.CameraManPhotoCount),
                        CustomerMobilePhotoCount = g.Sum(x => x.reservation.CustomerMobilePhotoCount),
                    };
                })
                .ToListAsync();
        }

        private async Task SendConfirmedPhotoShootPushNotificationAsync(int[] customerIdsToSend)
        {
            var tokens = await _pushNotificationTokenService.GetTokensAsync(customerIds: customerIdsToSend);


            var localizedTitle = await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.ScannerBoy.PushNotification.Title");

            var notificationTitle = string.IsNullOrEmpty(localizedTitle)
                ? $"New PhotoShoot!"
                : localizedTitle;


            var localizedDescription =
                await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.ScannerBoy.PushNotification.Description");
            var notificationBody = string.IsNullOrEmpty(localizedDescription)
                ? $"You have a new photoshoot!"
                : localizedDescription;


            await _pushNotificationSenderService.SendAsync(new Notification
            {
                Title = notificationTitle,
                Body = notificationBody,
                RegistrationIds = tokens,
                NotificationPlatform = NotificationPlatform.All
            });
        }


        public async Task<BaseServiceResult> UpdateReservationUsedCountsAsync(UpdateReservationApiParams apiParams)
        {
            var query =
                from reservationItem in _reservationItemRepository.Table
                join order in _orderRepository.Table on reservationItem.OrderId equals order.Id
                where apiParams.Items.Select(i => i.ReservationId).Contains(reservationItem.Id)
                select new { reservationItem, order };

            var result = await query.ToListAsync();

            if (result.Any() == false)
                return new BaseServiceResult("No reservations were found to update");


            if (result.Any(item => item.order.PaymentStatusId != (int)PaymentStatus.Paid))
                return new BaseServiceResult($"You can not update an unpaid reservation");

            if (result.Any(item => item.reservationItem.ReservationStatusId is (int)ReservationStatus.Complete or (int)ReservationStatus.Cancelled))
                return new BaseServiceResult($"You can not update a complete or canceled reservation");


            var diffs = apiParams.Items
                .Select(i => i.ReservationId)
                .Except(result.Select(i => i.reservationItem.Id))
                .ToArray();

            if (diffs.Any())

                return new BaseServiceResult($"Invalid reservationIds: {string.Join(",", diffs)}");


            var actorIds = new List<int>();
            var eventIds = new List<int>();

            foreach (var item in result)
            {
                var apiParamsItem = apiParams.Items.First(i => i.ReservationId == item.reservationItem.Id);

                if (item.reservationItem.UsedCameraManPhotoCount + apiParamsItem.UsedCameraManPhotoCount > item.reservationItem.CameraManPhotoCount ||
                    item.reservationItem.UsedCustomerMobilePhotoCount + apiParamsItem.UsedCustomerMobilePhotoCount >
                    item.reservationItem.CustomerMobilePhotoCount)
                {
                    return new BaseServiceResult(
                        $"Used photo counts should not become more than reservation photo counts, ReservationId:{item.reservationItem.Id}");
                }

                item.reservationItem.UsedCameraManPhotoCount += apiParamsItem.UsedCameraManPhotoCount;
                item.reservationItem.UsedCustomerMobilePhotoCount += apiParamsItem.UsedCustomerMobilePhotoCount;

                if (item.reservationItem.UsedCameraManPhotoCount == item.reservationItem.CameraManPhotoCount &&
                    item.reservationItem.UsedCustomerMobilePhotoCount == item.reservationItem.CustomerMobilePhotoCount)
                {
                    item.reservationItem.ReservationStatusId = (int)ReservationStatus.Complete;
                }

                actorIds.Add(item.reservationItem.ActorId);
                eventIds.Add(item.reservationItem.EventId);

                await _reservationItemRepository.UpdateAsync(item.reservationItem);
            }


            var productionEvents = await _productionEventService.GetEventsProductionRoles(eventIds.ToArray());
            var customerIdsToSend = new List<int>();

            customerIdsToSend.AddRange(actorIds);
            customerIdsToSend.AddRange(productionEvents.Select(x => x.CustomerId).ToList());

            await SendConfirmedPhotoShootPushNotificationAsync(customerIdsToSend.ToArray());


            return new BaseServiceResult();
        }


        public async Task<BaseServiceResult> ChangeReservationPhotographyDetailsAsync(
            ReservationItem reservationItem,
            ChangePhotographyDetailsApiParams apiParams)
        {
            var totalPhotoCounts = reservationItem.CameraManPhotoCount + reservationItem.CustomerMobilePhotoCount;
            var usedPhotoCounts = reservationItem.UsedCameraManPhotoCount + reservationItem.UsedCustomerMobilePhotoCount;

            if (totalPhotoCounts == usedPhotoCounts)
                return new BaseServiceResult($"All photography items are used for reservation with Id: {reservationItem.Id}");

            if (apiParams.NewActorId == reservationItem.ActorId)
            {
                var result = await HandleSameActorSwitchAsync(reservationItem, apiParams);

                if (result.IsSuccess == false)
                    return result;
            }
            else
            {
                var actorExists = await _actorEventService.GetActorEventAsync(reservationItem.EventId, apiParams.NewActorId);
                if (actorExists == null)
                    return new BaseServiceResult("Provided actor is not associated with this event");

                var otherReservationItem = await GetOtherActiveReservationAsync(reservationItem, apiParams.NewActorId);
                if (otherReservationItem != null)
                {
                    var result = await SwitchBetweenReservationsAsync(reservationItem, otherReservationItem, apiParams);

                    if (result.IsSuccess == false)
                        return result;
                }
                else
                {
                    var result = await CreateNewReservationWithSwitchedCountsAsync(reservationItem, apiParams);

                    if (result.IsSuccess == false)
                        return result;
                }
            }

            if (reservationItem.CameraManPhotoCount == 0 && reservationItem.CustomerMobilePhotoCount == 0)
                await _reservationItemRepository.DeleteAsync(reservationItem);

            return new BaseServiceResult();
        }

        private async Task<BaseServiceResult> HandleSameActorSwitchAsync(
            ReservationItem reservation,
            ChangePhotographyDetailsApiParams apiParams)
        {
            if (apiParams.SwitchCameraManPhotoCount)
            {
                var result = ValidatePhotoCount(reservation.CameraManPhotoCount, reservation.UsedCameraManPhotoCount,
                    nameof(reservation.CameraManPhotoCount));
                if (result.IsSuccess == false)
                    return result;
                reservation.CameraManPhotoCount--;
                reservation.CustomerMobilePhotoCount++;
            }

            if (apiParams.SwitchCustomerMobilePhotoCount)
            {
                var result = ValidatePhotoCount(reservation.CustomerMobilePhotoCount, reservation.UsedCustomerMobilePhotoCount,
                    nameof(reservation.CustomerMobilePhotoCount));
                if (result.IsSuccess == false)
                    return result;
                reservation.CustomerMobilePhotoCount--;
                reservation.CameraManPhotoCount++;
            }

            await _reservationItemRepository.UpdateAsync(reservation);
            return new BaseServiceResult();
        }

        public Task<ReservationItem> GetOtherActiveReservationAsync(ReservationItem baseItem, int newActorId)
        {
            return _reservationItemRepository.Table
                .FirstOrDefaultAsync(r =>
                    r.OrderId == baseItem.OrderId &&
                    r.EventId == baseItem.EventId &&
                    r.TimeSlotId == baseItem.TimeSlotId &&
                    r.Id != baseItem.Id &&
                    r.ActorId == newActorId &&
                    r.ReservationStatusId != (int)ReservationStatus.Complete);
        }

        public Task<List<ReservationItem>> GetOtherActiveReservationsAsync(ReservationItem baseItem)
        {
            return _reservationItemRepository.Table
                .Where(r =>
                    r.OrderId == baseItem.OrderId &&
                    r.EventId == baseItem.EventId &&
                    r.TimeSlotId == baseItem.TimeSlotId &&
                    r.Id != baseItem.Id)
                .ToListAsync();
        }

        private async Task<BaseServiceResult> SwitchBetweenReservationsAsync(ReservationItem source, ReservationItem target,
            ChangePhotographyDetailsApiParams apiParams)
        {
            if (apiParams.SwitchCameraManPhotoCount)
            {
                var result = ValidatePhotoCount(source.CameraManPhotoCount, source.UsedCameraManPhotoCount, nameof(source.CameraManPhotoCount));
                if (result.IsSuccess == false)
                    return result;
                source.CameraManPhotoCount--;
                target.CameraManPhotoCount++;
            }

            if (apiParams.SwitchCustomerMobilePhotoCount)
            {
                var result = ValidatePhotoCount(
                    source.CustomerMobilePhotoCount,
                    source.UsedCustomerMobilePhotoCount,
                    nameof(source.CustomerMobilePhotoCount)
                );
                if (result.IsSuccess == false)
                    return result;
                source.CustomerMobilePhotoCount--;
                target.CustomerMobilePhotoCount++;
            }

            await _reservationItemRepository.UpdateAsync(source);
            await _reservationItemRepository.UpdateAsync(target);
            return new BaseServiceResult();
        }

        private async Task<BaseServiceResult> CreateNewReservationWithSwitchedCountsAsync(
            ReservationItem source,
            ChangePhotographyDetailsApiParams apiParams)
        {
            var newItem = new ReservationItem
            {
                ActorId = apiParams.NewActorId,
                EventId = source.EventId,
                TimeSlotId = source.TimeSlotId,
                OrderId = source.OrderId,
                ReservationStatusId = (int)ReservationStatus.Processing,
                Queue = source.Queue,
                CameraManPhotoCount = 0,
                CustomerMobilePhotoCount = 0
            };

            if (apiParams.SwitchCameraManPhotoCount)
            {
                var result = ValidatePhotoCount(source.CameraManPhotoCount, source.UsedCameraManPhotoCount, nameof(source.CameraManPhotoCount));

                if (result.IsSuccess == false)
                    return result;

                source.CameraManPhotoCount--;
                newItem.CameraManPhotoCount = 1;
            }

            if (apiParams.SwitchCustomerMobilePhotoCount)
            {
                var result = ValidatePhotoCount(
                    source.CustomerMobilePhotoCount,
                    source.UsedCustomerMobilePhotoCount,
                    nameof(source.CustomerMobilePhotoCount));

                if (result.IsSuccess == false)
                    return result;

                source.CustomerMobilePhotoCount--;
                newItem.CustomerMobilePhotoCount = 1;
            }

            await _reservationItemRepository.UpdateAsync(source);
            await _reservationItemRepository.InsertAsync(newItem);

            return new BaseServiceResult();
        }

        private static BaseServiceResult ValidatePhotoCount(int count, int used, string fieldName)
        {
            return count - 1 < used
                ? new BaseServiceResult($"New {fieldName} value should not be less than Used{fieldName}")
                : new BaseServiceResult();
        }
    }
}
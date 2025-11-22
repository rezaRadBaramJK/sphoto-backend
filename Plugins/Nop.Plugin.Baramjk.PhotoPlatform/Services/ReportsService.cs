using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Tickets;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.CashierEvent;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class ReportsService
    {
        private readonly IRepository<ReservationItem> _reservationItemRepository;

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Actor> _actorRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<EventDetail> _eventDetailRepository;
        private readonly IRepository<TimeSlot> _timeSlotRepository;
        private readonly IRepository<CashierEvent> _cashierEventRepository;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<ActorEvent> _actorEventRepository;
        private readonly IRepository<GatewayPaymentTranslation> _gatewayPaymentTranslationRepository;
        private readonly IRepository<WalletHistory> _walletHistoryRepository;
        private readonly IRepository<ProductionEvent> _productionEventRepository;
        private readonly IRepository<SupervisorEvent> _supervisorEventRepository;

        public ReportsService(IRepository<ReservationItem> reservationItemRepository,
            IRepository<Order> orderRepository,
            IRepository<Actor> actorRepository,
            IRepository<Product> productRepository,
            IRepository<EventDetail> eventDetailRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            IRepository<TimeSlot> timeSlotRepository,
            IRepository<CashierEvent> cashierEventRepository,
            IRepository<Customer> customerRepository,
            IRepository<ActorEvent> actorEventRepository,
            IRepository<GatewayPaymentTranslation> gatewayPaymentTranslationRepository,
            IRepository<WalletHistory> walletHistoryRepository,
            IRepository<ProductionEvent> productionEventRepository,
            IRepository<SupervisorEvent> supervisorEventRepository)
        {
            _reservationItemRepository = reservationItemRepository;
            _orderRepository = orderRepository;
            _actorRepository = actorRepository;
            _productRepository = productRepository;
            _eventDetailRepository = eventDetailRepository;
            _genericAttributeRepository = genericAttributeRepository;
            _timeSlotRepository = timeSlotRepository;
            _cashierEventRepository = cashierEventRepository;
            _customerRepository = customerRepository;
            _actorEventRepository = actorEventRepository;
            _gatewayPaymentTranslationRepository = gatewayPaymentTranslationRepository;
            _walletHistoryRepository = walletHistoryRepository;
            _productionEventRepository = productionEventRepository;
            _supervisorEventRepository = supervisorEventRepository;
        }

        public Task<List<ReservationDetailsModel>> GetEventTimeSlotReservationsAsync(int eventId, int timeSlotId)
        {
            var query =
                from reservationItem in _reservationItemRepository.Table
                join actor in _actorRepository.Table on reservationItem.ActorId equals actor.Id
                join timeSlot in _timeSlotRepository.Table on reservationItem.TimeSlotId equals timeSlot.Id
                join product in _productRepository.Table on reservationItem.EventId equals product.Id
                join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                where reservationItem.EventId == eventId && reservationItem.TimeSlotId == timeSlotId
                select new ReservationDetailsModel
                {
                    ReservationItem = reservationItem,
                    Actor = actor,
                    TimeSlot = timeSlot,
                    Product = product,
                };


            return query.ToListAsync();
        }

        public Task<List<ReservationDetailsModel>> GetAllEventReservationsAsync(int eventId)
        {
            var query =
                from reservationItem in _reservationItemRepository.Table
                where reservationItem.EventId == eventId
                join actor in _actorRepository.Table on reservationItem.ActorId equals actor.Id
                join timeSlot in _timeSlotRepository.Table on reservationItem.TimeSlotId equals timeSlot.Id
                join product in _productRepository.Table on reservationItem.EventId equals product.Id
                join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                where timeSlot.Date.Date >= eventDetail.StartDate.Date
                select new ReservationDetailsModel
                {
                    ReservationItem = reservationItem,
                    Actor = actor,
                    TimeSlot = timeSlot,
                    Product = product,
                    EventDetail = eventDetail,
                };


            return query.ToListAsync();
        }

        public Task<List<CashierEventReportDetailModel>> GetCashierEventReservationsForCashierReportAsync(
            int eventId,
            int cashierId,
            DateTime eventDate,
            int? timeSlotId = null)
        {
            var query =
                from ga in _genericAttributeRepository.Table
                where ga.Key == DefaultValues.OrderPlacedByCashierAttributeKey && ga.Value == cashierId.ToString()
                join order in _orderRepository.Table on ga.EntityId equals order.Id
                where order.Deleted == false
                join reservation in _reservationItemRepository.Table
                    on order.Id equals reservation.OrderId into reservationGroup
                from reservationItem in reservationGroup.DefaultIfEmpty()
                where reservationItem.EventId == eventId
                join product in _productRepository.Table
                    on reservationItem.EventId equals product.Id
                join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                join timeSlot in _timeSlotRepository.Table
                    on reservationItem.TimeSlotId equals timeSlot.Id
                where timeSlot.Date == eventDate
                join cashierEvent in _cashierEventRepository.Table
                    on new { CustomerId = ga.Value, reservationItem.EventId }
                    equals new { CustomerId = cashierEvent.CustomerId.ToString(), cashierEvent.EventId }
                where cashierEvent.Deleted == false
                select new CashierEventReportDetailModel
                {
                    Order = order,
                    Reservation = reservationItem,
                    EventDetail = eventDetail,
                    Product = product,
                    TimeSlot = timeSlot,
                    CashierEvent = cashierEvent,
                };

            if (timeSlotId != null)
            {
                query = query.Where(item => item.TimeSlot.Id == timeSlotId);
            }


            return query.ToListAsync();
        }

        public Task<List<CashierEventReportDetailModel>> GetOnlineEventReservationsForCashierReportAsync(
            int eventId,
            int cashierId,
            DateTime eventDate,
            int? timeSlotId = null)
        {
            var query =
                from order in _orderRepository.Table
                where order.Deleted == false && order.CustomerId != cashierId && order.PaymentStatusId == (int)PaymentStatus.Paid
                join reservation in _reservationItemRepository.Table
                    on order.Id equals reservation.OrderId into reservationGroup
                from reservationItem in reservationGroup.DefaultIfEmpty()
                where reservationItem.EventId == eventId
                join product in _productRepository.Table
                    on reservationItem.EventId equals product.Id
                join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                join timeSlot in _timeSlotRepository.Table
                    on reservationItem.TimeSlotId equals timeSlot.Id
                where timeSlot.Date == eventDate
                select new CashierEventReportDetailModel
                {
                    Order = order,
                    Reservation = reservationItem,
                    EventDetail = eventDetail,
                    Product = product,
                    TimeSlot = timeSlot,
                };

            if (timeSlotId != null)
            {
                query = query.Where(item => item.TimeSlot.Id == timeSlotId);
            }


            return query.ToListAsync();
        }


        public async Task<List<TicketReservationDetailsModel>> GetTicketReportDetailsAsync(List<int> eventIds, DateTime? fromDate, DateTime? toDate,
            TimeSpan? startTime)
        {
            var query =
                from reservationItem in _reservationItemRepository.Table
                join actor in _actorRepository.Table on reservationItem.ActorId equals actor.Id
                join order in _orderRepository.Table on reservationItem.OrderId equals order.Id
                join timeSlot in _timeSlotRepository.Table on reservationItem.TimeSlotId equals timeSlot.Id
                join product in _productRepository.Table on reservationItem.EventId equals product.Id
                join eventDetail in _eventDetailRepository.Table on product.Id equals eventDetail.EventId
                join customer in _customerRepository.Table on order.CustomerId equals customer.Id
                join actorEvent in _actorEventRepository.Table on new { eventDetail.EventId, ActorId = actor.Id } equals new
                    { actorEvent.EventId, actorEvent.ActorId }
                join ga in _genericAttributeRepository.Table
                        .Where(ga => ga.Key == DefaultValues.OrderPlacedByCashierAttributeKey)
                    on order.Id equals ga.EntityId into gaGroup
                from ga in gaGroup.DefaultIfEmpty()
                join cashierEvent in _cashierEventRepository.Table
                    on new { CustomerId = ga != null ? ga.Value : null, reservationItem.EventId }
                    equals new { CustomerId = cashierEvent.CustomerId.ToString(), cashierEvent.EventId } into cashierEventGroup
                from cashierEvent in cashierEventGroup.DefaultIfEmpty()
                join gateway in _gatewayPaymentTranslationRepository.Table
                    on order.Id equals gateway.ConsumerEntityId into gatewayGroup
                from gateway in gatewayGroup.DefaultIfEmpty()
                join wallet in _walletHistoryRepository.Table.Where(wh => wh.WalletHistoryType == WalletHistoryType.Withdrawal)
                    on order.Id equals wallet.OriginatedEntityId into walletGroup
                where eventIds.Contains(reservationItem.EventId)
                      && (fromDate == null || timeSlot.Date >= fromDate)
                      && (toDate == null || timeSlot.Date <= toDate)
                      && (startTime == null || timeSlot.StartTime >= startTime)
                      && (cashierEvent == null || cashierEvent.Deleted == false)
                select new
                {
                    reservationItem,
                    actor,
                    timeSlot,
                    product,
                    eventDetail,
                    order,
                    customer,
                    cashierEvent,
                    actorEvent,
                    gateway,
                    WalletUsedAmount = walletGroup.Sum(w => w.Amount)
                };


            var queryResult = await query.ToListAsync();

            var grouped = queryResult
                .GroupBy(x => new { OrderId = x.order.Id, TimeSlotId = x.timeSlot.Id })
                .Select(g => new TicketReservationDetailsModel
                {
                    Order = g.First().order,
                    Customer = g.First().customer,
                    CashierEvent = g.FirstOrDefault()?.cashierEvent,
                    ReservationItem = g.First().reservationItem,
                    TimeSlot = g.First().timeSlot,
                    Product = g.First().product,
                    EventDetail = g.First().eventDetail,
                    Actors = g.Select(x => x.actor).Distinct().ToList(),
                    ActorEvent = g.First().actorEvent,
                    MyFatoorahReference = g.First().gateway?.InvoiceId,
                    WalletUsedAmount = g.First().WalletUsedAmount
                })
                .ToList();

            return grouped;
        }

        

        

        public Task<List<ReservationDetailsModel>> GetProductionTicketRevenueDataAsync(int eventId, int customerId, List<int> actorIds, DateTime? date,
            int? timeSlotId = 0)
        {
            var query =
                from reservationItem in _reservationItemRepository.Table
                join order in _orderRepository.Table on reservationItem.OrderId equals order.Id
                where order.PaymentStatusId == (int)PaymentStatus.Paid
                join actor in _actorRepository.Table on reservationItem.ActorId equals actor.Id
                join timeSlot in _timeSlotRepository.Table on reservationItem.TimeSlotId equals timeSlot.Id
                join eventDetail in _eventDetailRepository.Table on reservationItem.EventId equals eventDetail.EventId
                join product in _productRepository.Table on reservationItem.EventId equals product.Id
                join productionEvent in _productionEventRepository.Table.Where(pe => pe.CustomerId == customerId && pe.Deleted == false)
                    on new { reservationItem.EventId, CustomerId = customerId }
                    equals new { productionEvent.EventId, productionEvent.CustomerId }
                join actorEvent in _actorEventRepository.Table
                    on new { reservationItem.EventId, ActorId = actor.Id }
                    equals new { actorEvent.EventId, actorEvent.ActorId }
                where reservationItem.EventId == eventId
                      && (actorIds.Any() == false || actorIds.Contains(reservationItem.ActorId))
                      && (date == null || timeSlot.Date.Date == date)
                      && (timeSlotId == null || timeSlot.Id == timeSlotId)
                select new ReservationDetailsModel
                {
                    Actor = actor,
                    ActorEvent = actorEvent,
                    ReservationItem = reservationItem,
                    TimeSlot = timeSlot,
                    EventDetail = eventDetail,
                    Product = product
                };

            return query.ToListAsync();
            
            
        }
        
        
        public Task<List<ReservationDetailsModel>> GetSupervisorTicketRevenueDataAsync(int eventId, int customerId, List<int> actorIds, DateTime? date,
            int? timeSlotId = 0)
        {
            var query =
                from reservationItem in _reservationItemRepository.Table
                join order in _orderRepository.Table on reservationItem.OrderId equals order.Id
                where order.PaymentStatusId == (int)PaymentStatus.Paid                join actor in _actorRepository.Table on reservationItem.ActorId equals actor.Id
                join timeSlot in _timeSlotRepository.Table on reservationItem.TimeSlotId equals timeSlot.Id
                join eventDetail in _eventDetailRepository.Table on reservationItem.EventId equals eventDetail.EventId
                join product in _productRepository.Table on reservationItem.EventId equals product.Id
                join supervisorEvent in _supervisorEventRepository.Table.Where(se => se.CustomerId == customerId && se.Deleted == false)
                    on new { reservationItem.EventId, CustomerId = customerId }
                    equals new { supervisorEvent.EventId, supervisorEvent.CustomerId }
                join actorEvent in _actorEventRepository.Table
                    on new { reservationItem.EventId, ActorId = actor.Id }
                    equals new { actorEvent.EventId, actorEvent.ActorId }
                where reservationItem.EventId == eventId
                      && (actorIds.Any() == false || actorIds.Contains(reservationItem.ActorId))
                      && (date == null || timeSlot.Date.Date == date)
                      && (timeSlotId == null || timeSlot.Id == timeSlotId)
                select new ReservationDetailsModel
                {
                    Actor = actor,
                    ActorEvent = actorEvent,
                    ReservationItem = reservationItem,
                    TimeSlot = timeSlot,
                    EventDetail = eventDetail,
                    Product = product
                };

            return query.ToListAsync();
            
            
        }
        

    }
}
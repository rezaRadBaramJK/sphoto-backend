using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Cashier.Order;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.CashierBalanceHistory;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Services;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class PhotoPlatformOrderService
    {
        private readonly IRepository<ReservationItem> _reservationItemRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ShoppingCartItemTimeSlot> _shoppingCartItemTimeSlotRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly IWalletService _walletService;
        private readonly IWorkContext _workContext;
        private readonly CashierEventService _cashierEventService;
        private readonly ReservationItemService _reservationItemService;
        private readonly CashierBalanceService _cashierBalanceService;
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;


        public PhotoPlatformOrderService(IRepository<ReservationItem> reservationItemRepository,
            IRepository<ShoppingCartItemTimeSlot> shoppingCartItemTimeSlotRepository,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IOrderProcessingService orderProcessingService,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            IWalletService walletService,
            IWorkContext workContext,
            CashierEventService cashierEventService,
            ReservationItemService reservationItemService,
            CashierBalanceService cashierBalanceService,
            ILogger logger,
            IRepository<Order> orderRepository,
            ICustomerService customerService)
        {
            _reservationItemRepository = reservationItemRepository;
            _shoppingCartItemTimeSlotRepository = shoppingCartItemTimeSlotRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _orderProcessingService = orderProcessingService;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
            _walletService = walletService;
            _workContext = workContext;
            _cashierEventService = cashierEventService;
            _reservationItemService = reservationItemService;
            _cashierBalanceService = cashierBalanceService;
            _logger = logger;
            _orderRepository = orderRepository;
            _customerService = customerService;
        }

        public async Task MoveShoppingCartItemsToOrderItemsAsync(Order order)
        {
            //! We must use CustomerId of the order because the workContextCustomer may be different
            var query =
                from scit in _shoppingCartItemTimeSlotRepository.Table
                join shoppingCartItem in _shoppingCartItemRepository.Table on scit.ShoppingCartItemId equals shoppingCartItem.Id
                where shoppingCartItem.CustomerId == order.CustomerId && shoppingCartItem.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart
                select new
                {
                    scit,
                    shoppingCartItem.Quantity,
                    ItemId = shoppingCartItem.Id,
                    shoppingCartItem.ProductId,
                };


            var shoppingCartItemTimeSlots = await query.ToListAsync();

            var groupedShoppingCartItemTimeSlots = shoppingCartItemTimeSlots
                .GroupBy(item => item.ItemId)
                .Select(grouping => new
                {
                    grouping.First().Quantity,
                    ShoppingCartItemTimeSlots = grouping.ToArray()
                }).ToArray();

            if (groupedShoppingCartItemTimeSlots.Any(item => item.Quantity != item.ShoppingCartItemTimeSlots.Length))
            {
                await _logger.ErrorAsync(
                    $"moving shopping cart items to order items failed. groupedShoppingCartItemTimeSlots:{JsonConvert.SerializeObject(groupedShoppingCartItemTimeSlots)}");
                throw new NopException("Failed to move shopping cart items to order items");
            }


            // check if the customer associated with the order is cashier or not - customers will have same queue numbers for same timeSlots on different orders
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            var isOrderCustomerCashier = await _customerService.IsInCustomerRoleAsync(customer, DefaultValues.CashierRoleName);

            var matchingQueueNumbers = new Dictionary<int, int>();

            if (isOrderCustomerCashier == false)
            {
                var eventTimeSlotPairs = shoppingCartItemTimeSlots
                    .Select(i => new { EventId = i.ProductId, i.scit.TimeSlotId })
                    .Distinct()
                    .ToHashSet();

                var prevReservationFromDifferentOrderQuery =
                    from ri in _reservationItemRepository.Table
                    join o in _orderRepository.Table on ri.OrderId equals o.Id
                    where o.CustomerId == order.CustomerId &&
                          o.Id != order.Id
                    select new { ri.EventId, ri.TimeSlotId, ri.Queue };
                var prevReservationFromDifferentOrder = await prevReservationFromDifferentOrderQuery.ToListAsync();

                matchingQueueNumbers = prevReservationFromDifferentOrder
                    .Where(r => eventTimeSlotPairs.Contains(new { r.EventId, r.TimeSlotId }))
                    .GroupBy(r => r.TimeSlotId)
                    .ToDictionary(g => g.Key, g => g.First().Queue);
            }


            var allTimeSlotReservations = await _reservationItemRepository.Table
                .Where(r => shoppingCartItemTimeSlots.Select(s => s.scit.TimeSlotId).Contains(r.TimeSlotId))
                .ToListAsync();

            var previousReservationItems = allTimeSlotReservations
                .GroupBy(r => r.TimeSlotId)
                .Select(g => new
                {
                    TimeSlotId = g.Key,
                    QueueNumber = g.Max(x => x.Queue) + 1,
                })
                .ToDictionary(
                    item => item.TimeSlotId,
                    item => item.QueueNumber
                );


            var reservationItems = shoppingCartItemTimeSlots
                .Select(item =>
                {
                    var prevQueue = matchingQueueNumbers.TryGetValue(item.scit.TimeSlotId, out var prevQueueNumber) ? prevQueueNumber : 0;

                    if (prevQueue != 0)
                    {
                        return new ReservationItem
                        {
                            TimeSlotId = item.scit.TimeSlotId,
                            OrderId = order.Id,
                            EventId = item.ProductId,
                            ActorId = item.scit.ActorId,
                            CameraManPhotoCount = item.scit.CameraManPhotoCount,
                            CustomerMobilePhotoCount = item.scit.CustomerMobilePhotoCount,
                            ReservationStatusId = (int)ReservationStatus.Processing,
                            Queue = prevQueue,
                        };
                    }

                    var currentQueue = previousReservationItems.TryGetValue(item.scit.TimeSlotId, out var quantity)
                        ? quantity
                        : 1;

                    if (previousReservationItems.TryAdd(item.scit.TimeSlotId, 1) == false)
                    {
                        previousReservationItems[item.scit.TimeSlotId] = currentQueue;
                    }

                    return new ReservationItem
                    {
                        TimeSlotId = item.scit.TimeSlotId,
                        OrderId = order.Id,
                        EventId = item.ProductId,
                        ActorId = item.scit.ActorId,
                        CameraManPhotoCount = item.scit.CameraManPhotoCount,
                        CustomerMobilePhotoCount = item.scit.CustomerMobilePhotoCount,
                        ReservationStatusId = (int)ReservationStatus.Processing,
                        Queue = currentQueue,
                    };
                })
                .ToList();

            await _reservationItemRepository.InsertAsync(reservationItems);
        }


        public async Task<BaseServiceResult> MarkAsPaidAsync(Order order, Customer customer, CashierPaymentMethods paymentMethod)
        {
            if (order == null || order.Deleted)
                return new BaseServiceResult("Order not found.");

            if (order.OrderStatus == OrderStatus.Complete)
                return new BaseServiceResult("Order already completed.");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return new BaseServiceResult("Can't mark cancelled order as paid.");

            if (order.PaymentStatus == PaymentStatus.Paid)
                return new BaseServiceResult("This order already has been marked as paid.");


            await _orderProcessingService.MarkOrderAsPaidAsync(order);

            order.PaymentMethodSystemName = paymentMethod.ToString();

            await _orderService.UpdateOrderAsync(order);

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = $"Marked as paid by {customer.Email}",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            await _genericAttributeService.SaveAttributeAsync(order, DefaultValues.PaidWithAttributeKey, paymentMethod);

            return new BaseServiceResult();
        }


        private async Task<BaseServiceResult> IncreaseCashierFundBalanceAsync(int cashierId, decimal amount, int orderId)
        {
            var reservationItems = await _reservationItemService.GetAllOrderReservationsAsync(orderId);
            if (reservationItems.Any() == false)
                return new BaseServiceResult("There aren't any reservations for this order.");

            var eventId = reservationItems.First().EventId;
            var cashierEvent = await _cashierEventService.GetByCashierIdAndEventIdAsync(cashierId, eventId);
            if (cashierEvent == null)
                return new BaseServiceResult("There is no cashier event with the specified id.");


            var request = new CashierBalanceTransactionRequest
            {
                CashierEventId = cashierEvent.Id,
                Amount = amount,
                Type = CashierBalanceHistoryType.RefundedTicketByCash,
                Note = $"Cashier Refunded Ticket from order with id: {orderId}"
            };
            if (await _cashierBalanceService.CanPerformAsync(request) == false)
            {
                return new BaseServiceResult("Failed to refund to cashier fund balance.");
            }

            await _cashierBalanceService.PerformAsync(request);

            return new BaseServiceResult();
        }

        public async Task<BaseServiceResult> RefundOrderFromCashierAsync(Order order, CashierRefundMethod cashierRefundMethod)
        {
            var currentCashier = await _workContext.GetCurrentCustomerAsync();

            var cashierIdWhichPlacedOrder =
                await _genericAttributeService.GetAttributeAsync<int>(order, DefaultValues.OrderPlacedByCashierAttributeKey);

            if (cashierRefundMethod == CashierRefundMethod.Wallet)
            {
                // if the cashierIdWhichPlacedOrder was not 0 we know that the order was placed by a cashier
                if (cashierIdWhichPlacedOrder != 0)
                {
                    return new BaseServiceResult("You cannot use wallet refund method for an order which was placed by a cashier");
                }

                var walletTransactionRequest = new WalletTransactionRequest()
                {
                    CustomerId = order.CustomerId,
                    CurrencyCode = order.CustomerCurrencyCode,
                    Amount = order.OrderTotal,
                    Type = WalletHistoryType.Charge,
                    OriginatedEntityId = order.Id,
                    IsRevert = false,
                    Note = $"Refunded from cashier: {currentCashier.Email}",
                };
                var result = await _walletService.CanPerformAsync(walletTransactionRequest);

                if (result)
                {
                    await _walletService.PerformAsync(walletTransactionRequest);
                }
                else
                {
                    return new BaseServiceResult("Failed to perform wallet refund transaction.");
                }
            }
            else if (cashierRefundMethod == CashierRefundMethod.Cash)
            {
                if (cashierIdWhichPlacedOrder == 0)
                {
                    // increase the current cashier fund balance 
                    var result = await IncreaseCashierFundBalanceAsync(currentCashier.Id, order.OrderTotal, order.Id);

                    if (result.IsSuccess == false)
                        return result;
                }

                else
                {
                    // increase the cashier balance who placed the order 
                    var result = await IncreaseCashierFundBalanceAsync(cashierIdWhichPlacedOrder, order.OrderTotal, order.Id);

                    if (result.IsSuccess == false)
                        return result;
                }
            }
            else
            {
                return new BaseServiceResult("Refund Method is not valid");
            }

            order.OrderStatusId = (int)OrderStatus.Cancelled;
            order.PaymentStatusId = (int)PaymentStatus.Refunded;

            await _orderService.UpdateOrderAsync(order);
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                CreatedOnUtc = DateTime.UtcNow,
                DisplayToCustomer = false,
                Note = $"Order has been refunded. Refund method: {cashierRefundMethod}",
            });

            await _reservationItemService.ChangeReservationsStatusAsync(order.Id, (int)ReservationStatus.Cancelled);

            return new BaseServiceResult();
        }
    }
}
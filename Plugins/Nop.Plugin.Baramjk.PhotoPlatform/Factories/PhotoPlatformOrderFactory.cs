using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.Framework.Models.PagedLists;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Cashier.Order;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Order;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reservations;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;


namespace Nop.Plugin.Baramjk.PhotoPlatform.Factories
{
    public class PhotoPlatformOrderFactory
    {
        private readonly EventFactory _eventFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly ICurrencyService _currencyService;


        public PhotoPlatformOrderFactory(
            EventFactory eventFactory,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            ICurrencyService currencyService)
        {
            _eventFactory = eventFactory;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _workContext = workContext;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _currencyService = currencyService;
        }

        private ReservationStatusType GetReservationStatusType(TimeSlot timeSlot, Order order, List<ReservationDetailsModel> reservationDetailsModel)
        {
            if (order.PaymentStatusId == (int)PaymentStatus.Refunded)
            {
                return ReservationStatusType.Refunded;
            }

            if (reservationDetailsModel.All(i => i.ReservationItem.ReservationStatusId == (int)ReservationStatus.Complete))
                return ReservationStatusType.Used;

            if (timeSlot.Date + timeSlot.EndTime < DateTime.UtcNow)
            {
                return ReservationStatusType.Expired;
            }

            if (order.OrderStatusId == (int)OrderStatus.Pending && order.PaymentStatusId == (int)PaymentStatus.Pending)
            {
                return ReservationStatusType.NotPaid;
            }

            if (order.OrderStatusId == (int)OrderStatus.Complete && order.PaymentStatusId == (int)PaymentStatus.Paid)
            {
                return ReservationStatusType.Valid;
            }

            return ReservationStatusType.Invalid;
        }

        private bool IsReservationEditable(ReservationStatusType status, EventDetail eventDetail)
        {
            if (status is ReservationStatusType.Refunded or ReservationStatusType.Used or ReservationStatusType.NotPaid ||
                eventDetail.EndDate + eventDetail.EndTime < DateTime.Now) return false;
            return true;
        }

        public async Task<OrderDetailDto> PrepareOrderDetailDtoAsync(Order order, List<ReservationDetailsModel> reservationWithDetails,
            Customer customer = null, bool includeUsed = true, bool checkEditable = true, bool convertCurrency = false)
        {
            customer ??= await _workContext.GetCurrentCustomerAsync();
            var orderDto = order.Map<OrderDetailDto>();

            var customerTimeZone = await _dateTimeHelper.GetCustomerTimeZoneAsync(customer);


            orderDto.OrderGuid = order.OrderGuid.ToString();

            var groupedItems = reservationWithDetails
                .GroupBy(r => new { EventId = r.Product.Id, TimeSlotId = r.TimeSlot.Id })
                .Select(g => new
                {
                    g.Key.EventId,
                    g.Key.TimeSlotId,
                    Details = g.ToList()
                })
                .ToList();

            orderDto.Reservations = await groupedItems
                .SelectAwait(async ri =>
                {
                    var firstItem = ri.Details.First();
                    var status = GetReservationStatusType(firstItem.TimeSlot, order, ri.Details);
                    return new ReservationItemDto
                    {
                        EventId = ri.EventId,
                        EventName = await _localizationService.GetLocalizedAsync(firstItem.Product, p => p.Name),
                        TimeSlotId = ri.TimeSlotId,
                        ReservationDate = _dateTimeHelper
                            .ConvertToUserTime(firstItem.TimeSlot.Date, TimeZoneInfo.Utc, customerTimeZone)
                            .ToString("yyyy-MM-dd"),
                        ReservationTime = firstItem.TimeSlot.StartTime.ToString(),
                        Picture = await _eventFactory.PrepareEventPictureAsync(firstItem.Product),
                        Queue = firstItem.ReservationItem.Queue,
                        ReservationStatusId = (int)status,
                        IsEditable = checkEditable && IsReservationEditable(status, firstItem.EventDetail),
                        PhotographyDetails = await ri.Details.SelectAwait(async item => new PhotographyDetails
                            {
                                ReservationId = item.ReservationItem.Id,
                                ActorId = item.Actor.Id,
                                ActorName = await _localizationService.GetLocalizedAsync(item.Actor, a => a.Name),
                                CameraManPhotoCount = item.ReservationItem.CameraManPhotoCount,
                                CustomerMobilePhotoCount = item.ReservationItem.CustomerMobilePhotoCount,
                                UsedCameraManPhotoCount = item.ReservationItem.UsedCameraManPhotoCount,
                                UsedCustomerMobilePhotoCount = item.ReservationItem.UsedCustomerMobilePhotoCount,
                                CustomerMobileEachPictureCost =
                                    await PreparePriceAsync(customer, item.ActorEvent.CustomerMobileEachPictureCost, convertCurrency),

                                CameramanEachPictureCost =
                                    await PreparePriceAsync(customer, item.ActorEvent.CameraManEachPictureCost, convertCurrency),
                                ActorPictures = await _eventFactory.PrepareActorPicturesAsync(item.Actor)
                            })
                            .Where(pd => includeUsed || (pd.UsedCustomerMobilePhotoCount < pd.CustomerMobilePhotoCount ||
                                                         pd.UsedCameraManPhotoCount < pd.CameraManPhotoCount))
                            .ToListAsync()
                    };
                })
                .ToListAsync();

            orderDto.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, customerTimeZone);
            orderDto.PaymentMethod = order.PaymentMethodSystemName;
            orderDto.OrderSubtotal = await PreparePriceAsync(customer, order.OrderSubtotalExclTax, convertCurrency);
            orderDto.OrderSubTotalDiscount = await PreparePriceAsync(customer, order.OrderSubTotalDiscountExclTax, convertCurrency);
            orderDto.OrderTotalDiscount = await PreparePriceAsync(customer, order.OrderDiscount, convertCurrency);
            orderDto.OrderTotal = await PreparePriceAsync(customer, order.OrderTotal, convertCurrency);
            orderDto.PaymentMethod = !string.IsNullOrEmpty(orderDto.PaymentMethod) && orderDto.PaymentMethod.Contains(".")
                ? orderDto.PaymentMethod.Split('.').Last()
                : orderDto.PaymentMethod ?? string.Empty;
            orderDto.PhoneNumber =
                await _genericAttributeService.GetAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, defaultValue: string.Empty);


            return orderDto;
        }

        private async Task<string> PreparePriceAsync(Customer customer, decimal amount, bool convertCurrency = false)
        {
            if (!convertCurrency) return await _priceFormatter.FormatPriceAsync(amount);

            var customerCurrencyId = await _genericAttributeService
                .GetAttributeAsync<int>(customer, NopCustomerDefaults.CurrencyIdAttribute);

            var currency = customerCurrencyId == 0
                ? await _workContext.GetWorkingCurrencyAsync()
                : await _currencyService.GetCurrencyByIdAsync(customerCurrencyId);

            var convertedToUserCurrency = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount, currency);

            return await _priceFormatter.FormatPriceAsync(convertedToUserCurrency, true, currency);
        }

        public async Task<CashierOrderDetailDto> PrepareCashierOrderDetailDtoAsync(Order order, List<ReservationDetailsModel> reservationWithDetails,
            Customer customer = null)
        {
            customer ??= await _workContext.GetCurrentCustomerAsync();
            var orderDto = order.Map<CashierOrderDetailDto>();

            var customerTimeZone = await _dateTimeHelper.GetCustomerTimeZoneAsync(customer);

            orderDto.OrderGuid = order.OrderGuid.ToString();

            var groupedItems = reservationWithDetails
                .GroupBy(r => new { EventId = r.Product.Id, TimeSlotId = r.TimeSlot.Id })
                .Select(g => new
                {
                    g.Key.EventId,
                    g.Key.TimeSlotId,
                    Details = g.ToList()
                })
                .ToList();

            orderDto.Reservations = await groupedItems.SelectAwait(async ri =>
            {
                var firstItem = ri.Details.First();
                return new ReservationItemDto
                {
                    EventId = ri.EventId,
                    EventName = await _localizationService.GetLocalizedAsync(firstItem.Product, p => p.Name),
                    TimeSlotId = ri.TimeSlotId,
                    ReservationDate = _dateTimeHelper
                        .ConvertToUserTime(firstItem.TimeSlot.Date, TimeZoneInfo.Utc, customerTimeZone)
                        .ToString("yyyy-MM-dd"),
                    ReservationTime = firstItem.TimeSlot.StartTime.ToString(),
                    Picture = await _eventFactory.PrepareEventPictureAsync(firstItem.Product),
                    Queue = firstItem.ReservationItem.Queue,
                    ReservationStatusId = (int)GetReservationStatusType(firstItem.TimeSlot, order, ri.Details),
                    PhotographyDetails = await ri.Details.SelectAwait(async item => new PhotographyDetails
                    {
                        ReservationId = item.ReservationItem.Id,
                        ActorId = item.Actor.Id,
                        ActorName = await _localizationService.GetLocalizedAsync(item.Actor, a => a.Name),
                        CameraManPhotoCount = item.ReservationItem.CameraManPhotoCount,
                        CustomerMobilePhotoCount = item.ReservationItem.CustomerMobilePhotoCount,
                        UsedCameraManPhotoCount = item.ReservationItem.UsedCameraManPhotoCount,
                        UsedCustomerMobilePhotoCount = item.ReservationItem.UsedCustomerMobilePhotoCount,
                        CustomerMobileEachPictureCost = await _priceFormatter.FormatPriceAsync(item.ActorEvent.CustomerMobileEachPictureCost),
                        CameramanEachPictureCost = await _priceFormatter.FormatPriceAsync(item.ActorEvent.CustomerMobileEachPictureCost),
                        ActorPictures = await _eventFactory.PrepareActorPicturesAsync(item.Actor)
                    }).ToListAsync()
                };
            }).ToListAsync();

            orderDto.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, customerTimeZone);
            orderDto.PaymentMethod = order.PaymentMethodSystemName;
            orderDto.OrderSubtotal = await _priceFormatter.FormatPriceAsync(order.OrderSubtotalExclTax);
            orderDto.OrderSubTotalDiscount = await _priceFormatter.FormatPriceAsync(order.OrderSubTotalDiscountExclTax);
            orderDto.OrderTotalDiscount = await _priceFormatter.FormatPriceAsync(order.OrderDiscount);
            orderDto.OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal);
            orderDto.PaymentMethod = orderDto.PaymentMethod.Contains(".")
                ? orderDto.PaymentMethod.Split(".").Last()
                : orderDto.PaymentMethod;

            orderDto.PhoneNumber =
                await _genericAttributeService.GetAttributeAsync<string>(order, DefaultValues.CustomerPhoneForCashierOrderAttributeKey);

            var cashierIdWhichPlacedOrder =
                await _genericAttributeService.GetAttributeAsync<int>(order, DefaultValues.OrderPlacedByCashierAttributeKey);

            // when the order is placed online we add wallet to refund methods
            if (cashierIdWhichPlacedOrder == 0)
            {
                orderDto.CashierRefundOptionsIds.Add((int)CashierRefundMethod.Wallet);
            }

            if (cashierIdWhichPlacedOrder != 0)
            {
                orderDto.CashierName =
                    await _customerService.GetCustomerFullNameAsync(await _customerService.GetCustomerByIdAsync(cashierIdWhichPlacedOrder));
            }

            return orderDto;
        }


        public async Task<List<OrderDetailDto>> PrepareCustomerOrderListAsync(IPagedList<OrderReservationDetailsModel> reservationsWithDetails,
            bool checkEditable = true)

        {
            return await reservationsWithDetails
                .SelectAwait(
                    async r =>
                        await PrepareOrderDetailDtoAsync(r.Order, r.OrderReservationDetails, checkEditable: checkEditable, convertCurrency: true))
                .ToListAsync();
        }


        public async Task<CamelCasePagedList<CashierOrderDetailDto>> PrepareCashierOrderListAsync(
            IPagedList<OrderReservationDetailsModel> reservationsWithDetails)

        {
            var preparedItems = await reservationsWithDetails
                .SelectAwait(async r =>
                    await PrepareCashierOrderDetailDtoAsync(r.Order, r.OrderReservationDetails)
                )
                .ToListAsync();

            return new CamelCasePagedList<CashierOrderDetailDto>(
                preparedItems,
                reservationsWithDetails.PageIndex + 1,
                reservationsWithDetails.PageSize,
                reservationsWithDetails.TotalCount);
        }
    }
}
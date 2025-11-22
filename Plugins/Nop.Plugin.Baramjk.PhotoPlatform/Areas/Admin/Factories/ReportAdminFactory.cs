using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Cashier;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.EventCashier;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Sales;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Reports.Tickets;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Helpers;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Cashier.Order;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.CashierEvent;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;


namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class ReportAdminFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;


        public ReportAdminFactory(ILocalizationService localizationService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService)
        {
            _localizationService = localizationService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
        }

        public List<SelectListItem> PrepareEventDates(EventDetail eventDetail)
        {
            var datesBetween = DateTimeHelper.GetDatesBetween(eventDetail.StartDate, eventDetail.EndDate);

            return datesBetween.Select(date => new SelectListItem
            {
                Value = date.ToString("yyyy-MM-dd"),
                Text = date.ToString("yyyy-MM-dd")
            }).ToList();
        }

        public List<SelectListItem> PrepareMinAndMaxEventDates(List<EventDetail> eventsDetail)
        {
            var minStartDate = eventsDetail.Min(e => e.StartDate);
            var maxEndDate = eventsDetail.Max(e => e.EndDate);

            var datesBetween = DateTimeHelper.GetDatesBetween(minStartDate, maxEndDate);

            return datesBetween.Select(date => new SelectListItem
            {
                Value = date.ToString("yyyy-MM-dd"),
                Text = date.ToString("yyyy-MM-dd")
            }).ToList();
        }


        public List<SelectListItem> PrepareEventTimeSlots(List<TimeSlot> timeSlots)
        {
            return timeSlots.Select(t => t.StartTime).Distinct().Select(t => new SelectListItem
            {
                Value = t.ToString(),
                Text = t.ToString()
            }).ToList();
        }

        public List<SelectListItem> PrepareEventTimeSlotsForCashier(List<TimeSlot> timeSlots)
        {
            return timeSlots.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.StartTime.ToString(),
            }).ToList();
        }

        public List<SelectListItem> PrepareEventCashiersData(List<Customer> customers)
        {
            return customers
                .Select(customer => new SelectListItem
                {
                    Value = customer.Id.ToString(),
                    Text = customer.Email
                })
                .ToList();
        }


        public List<SelectListItem> PrepareEvents(List<Product> events)
        {
            var items = events
                .Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                }).ToList();
            items.Insert(0, new SelectListItem
            {
                Text = "Select Event",
                Value = "0"
            });
            return items;
        }


        public async Task<CashierReportPdfDataModel> PrepareCashierReportPdfData(List<ReservationDetailsModel> reservations, bool showTicketsCount)
        {
            var firstItem = reservations.First();
            var actorsData = reservations
                .GroupBy(r => r.ReservationItem.ActorId)
                .Select(g =>
                    new CashierReportPdfReservationDataModel
                    {
                        ActorName = g.ToList().First().Actor.Name,
                        VerifiedPhotosCount = g.ToList().Sum(r =>
                            r.ReservationItem.UsedCustomerMobilePhotoCount + r.ReservationItem.UsedCameraManPhotoCount)
                    }
                ).Where(d => d.VerifiedPhotosCount > 0)
                .ToList();

            return new CashierReportPdfDataModel
            {
                EventDate = firstItem.TimeSlot.Date.ToString("d"),
                EventTime = firstItem.TimeSlot.StartTime.ToString(@"hh\:mm"),
                EventName = await _localizationService.GetLocalizedAsync(firstItem.Product, p => p.Name),
                Reservations = actorsData,
                ShowTicketsCount = showTicketsCount,
            };
        }

        public SalesReportPdfDataModel PrepareSalesReportPdfData(List<ReservationDetailsModel> reservations,
            bool onlyShowConfirmedTickets = true)
        {
            return reservations
                .GroupBy(r => r.ReservationItem.EventId)
                .Select(eventGroup =>
                {
                    var totalTickets = onlyShowConfirmedTickets
                        ? eventGroup.Sum(r => r.ReservationItem.UsedCustomerMobilePhotoCount + r.ReservationItem.UsedCameraManPhotoCount)
                        : eventGroup.Sum(r => r.ReservationItem.CameraManPhotoCount + r.ReservationItem.CustomerMobilePhotoCount);
                    var groupedEventsFirstItem = eventGroup.First();
                    return new SalesReportPdfDataModel
                    {
                        EventName = groupedEventsFirstItem.Product.Name,
                        EachDayData = eventGroup
                            .GroupBy(r => new
                            {
                                DayNumber = (int)(r.TimeSlot.Date.Date - groupedEventsFirstItem.EventDetail.StartDate.Date).TotalDays + 1,
                            })
                            .OrderBy(g => g.Key.DayNumber)
                            .Select(dayGroup =>
                            {
                                var totalNumberOfTickets = onlyShowConfirmedTickets
                                    ? dayGroup.Sum(x => x.ReservationItem.UsedCameraManPhotoCount + x.ReservationItem.UsedCustomerMobilePhotoCount)
                                    : dayGroup.Sum(x => x.ReservationItem.CameraManPhotoCount + x.ReservationItem.CustomerMobilePhotoCount);
                                return new SalesEachDayDataPdfDataModel
                                {
                                    DayNumber = dayGroup.Key.DayNumber,
                                    TotalNumberOfTickets = totalNumberOfTickets,
                                    TotalPrice = totalNumberOfTickets * groupedEventsFirstItem.EventDetail.PhotoPrice,
                                    TotalProductionShare = totalNumberOfTickets * groupedEventsFirstItem.EventDetail.ProductionShare,
                                    TotalActorShare = totalNumberOfTickets * groupedEventsFirstItem.EventDetail.ActorShare,
                                    TotalPhotoShootShare = totalNumberOfTickets * groupedEventsFirstItem.EventDetail.PhotoShootShare,

                                    ReservationsDetails = dayGroup
                                        .GroupBy(dg => dg.Actor.Id)
                                        .Select(dayActorGroup =>
                                        {
                                            var totalDayActorTickets = onlyShowConfirmedTickets
                                                ? dayActorGroup.Sum(dg =>
                                                    dg.ReservationItem.UsedCameraManPhotoCount + dg.ReservationItem.UsedCustomerMobilePhotoCount)
                                                : dayActorGroup.Sum(dg =>
                                                    dg.ReservationItem.CameraManPhotoCount + dg.ReservationItem.CustomerMobilePhotoCount);
                                            var firstItem = dayActorGroup.First();
                                            return new SalesReservationDetailsDataModel
                                            {
                                                ActorName = firstItem.Actor.Name,
                                                NumberOfTickets = totalDayActorTickets,
                                                UnitPrice = firstItem.EventDetail.PhotoPrice,
                                                TotalPrice = totalDayActorTickets * firstItem.EventDetail.PhotoPrice,
                                                ProductionShare = totalDayActorTickets * firstItem.EventDetail.ProductionShare,
                                                ActorShare = totalDayActorTickets * firstItem.EventDetail.ActorShare,
                                                PhotoShootShare = totalDayActorTickets * firstItem.EventDetail.PhotoShootShare
                                            };
                                        })
                                        .Where(sr => sr.NumberOfTickets > 0)
                                        .ToList(),
                                };
                            }).ToList(),
                        TotalNumberOfTickets = totalTickets,
                        TotalPrice = totalTickets * groupedEventsFirstItem.EventDetail.PhotoPrice,
                        TotalProductionShare = totalTickets * groupedEventsFirstItem.EventDetail.ProductionShare,
                        TotalActorShare = totalTickets * groupedEventsFirstItem.EventDetail.ActorShare,
                        TotalPhotoShootShare = totalTickets * groupedEventsFirstItem.EventDetail.PhotoShootShare
                    };
                }).First();
        }


        public async Task<CashierEventReportPdfDataModel> PrepareCashierEventPdfData(
            List<CashierEventReportDetailModel> cashierReservations,
            List<CashierEventReportDetailModel> onlineReservations,
            bool includeTimeSlot,
            decimal cashierBalance = 0)
        {
            var firstItem = cashierReservations.First();
            var customer = await _customerService.GetCustomerByIdAsync(firstItem.CashierEvent.CustomerId);


            var pdfDataModel = new CashierEventReportPdfDataModel
            {
                EventName = await _localizationService.GetLocalizedAsync(firstItem.Product, p => p.Name),
                CashierName = await _customerService.GetCustomerFullNameAsync(customer),
                DayDateTime = firstItem.TimeSlot.Date,
                TimeSlot = firstItem.TimeSlot.StartTime
            };

            //online reservations
            var groupedOnlineReservations = onlineReservations.GroupBy(r => r.Order.Id);
            foreach (var group in groupedOnlineReservations)
            {
                var firstItemOnlineGroupedItem = group.First();
                pdfDataModel.TotalCameraManPhotoCount += group.Sum(r => r.Reservation.CameraManPhotoCount);
                pdfDataModel.TotalCustomerMobilePhotoCount += group.Sum(r => r.Reservation.CustomerMobilePhotoCount);
                pdfDataModel.TotalCameraManPhotoPrice +=
                    group.Sum(r => r.Reservation.CameraManPhotoCount * r.EventDetail.PhotoPrice);
                pdfDataModel.TotalCustomerMobilePhotoPrice +=
                    group.Sum(r => r.Reservation.CustomerMobilePhotoCount * r.EventDetail.PhotoPrice);
                pdfDataModel.TotalOnlinePayments += firstItemOnlineGroupedItem.Order.OrderTotal;
            }

            // cashier reservations
            var groupedItems = cashierReservations.GroupBy(r => r.Order.Id);

            foreach (var orderGroupedReservations in groupedItems)
            {
                var firstDetailedItem = orderGroupedReservations.First();


                if (firstDetailedItem.Order.PaymentStatusId == (int)PaymentStatus.Refunded)
                {
                    pdfDataModel.TotalRefundedPhotoCount +=
                        orderGroupedReservations.Sum(r => r.Reservation.CustomerMobilePhotoCount + r.Reservation.CameraManPhotoCount);
                    pdfDataModel.TotalRefundedPrice +=
                        orderGroupedReservations.Sum(r =>
                            r.Reservation.CustomerMobilePhotoCount * r.EventDetail.PhotoPrice +
                            r.Reservation.CameraManPhotoCount * r.EventDetail.PhotoPrice);
                }

                else

                {
                    pdfDataModel.TotalCameraManPhotoCount += orderGroupedReservations.Sum(r => r.Reservation.CameraManPhotoCount);
                    pdfDataModel.TotalCustomerMobilePhotoCount += orderGroupedReservations.Sum(r => r.Reservation.CustomerMobilePhotoCount);
                    pdfDataModel.TotalCameraManPhotoPrice +=
                        orderGroupedReservations.Sum(r => r.Reservation.CameraManPhotoCount * r.EventDetail.PhotoPrice);
                    pdfDataModel.TotalCustomerMobilePhotoPrice +=
                        orderGroupedReservations.Sum(r => r.Reservation.CustomerMobilePhotoCount * r.EventDetail.PhotoPrice);

                    if (firstDetailedItem.Order.PaymentMethodSystemName == nameof(CashierPaymentMethods.Cash))
                    {
                        pdfDataModel.TotalCashPayments += firstDetailedItem.Order.OrderTotal;
                    }
                    else if (firstDetailedItem.Order.PaymentMethodSystemName == nameof(CashierPaymentMethods.KNet))
                    {
                        pdfDataModel.TotalKNetPayments += firstDetailedItem.Order.OrderTotal;
                    }
                }
            }

            pdfDataModel.TotalPhotoCount = pdfDataModel.TotalCameraManPhotoCount + pdfDataModel.TotalCustomerMobilePhotoCount;
            pdfDataModel.TotalPhotoPrice = pdfDataModel.TotalCameraManPhotoPrice + pdfDataModel.TotalCustomerMobilePhotoPrice;
            pdfDataModel.PhotoCountNetProfit = pdfDataModel.TotalPhotoCount - pdfDataModel.TotalRefundedPhotoCount;
            pdfDataModel.TotalPhotoPriceNetProfit = pdfDataModel.TotalPhotoPrice - pdfDataModel.TotalRefundedPrice;

            pdfDataModel.OpeningFundBalance = cashierBalance;
            pdfDataModel.TotalFundBalance = pdfDataModel.TotalCashPayments - cashierBalance;
            pdfDataModel.TotalNetProfit = pdfDataModel.TotalPhotoPriceNetProfit - pdfDataModel.TotalOnlinePayments;
            pdfDataModel.IncludeTimeSlot = includeTimeSlot;


            return pdfDataModel;
        }


        private static string ClassifyPaymentMethod(string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
                return ReportPaymentMethods.None.ToString();

            paymentMethod = paymentMethod.Trim();

            return paymentMethod switch
            {
                var pm when pm.Contains("Myfatoorah", StringComparison.OrdinalIgnoreCase)
                            || pm.Contains("Wallet", StringComparison.OrdinalIgnoreCase)
                    => ReportPaymentMethods.Online.ToString(),

                var pm when pm.Contains("KNet", StringComparison.OrdinalIgnoreCase)
                    => ReportPaymentMethods.KNet.ToString(),

                var pm when pm.Contains("Cash", StringComparison.OrdinalIgnoreCase)
                    => ReportPaymentMethods.Cash.ToString(),

                _ => ReportPaymentMethods.None.ToString()
            };
        }

        private decimal CalculateShare(decimal photoPrice, decimal share, decimal photoCount)
        {
            return ((photoCount * share) / (photoCount * photoPrice)) * 100;
        }

        public async Task<List<TicketsReportExportModel>> PrepareTicketsReportExportModelAsync(List<TicketReservationDetailsModel> listModel)
        {
            var cashiers = await _customerService.GetCustomersByIdsAsync(listModel.Select(m => m.CashierEvent?.CustomerId ?? 0).Distinct().ToArray());

            return await listModel
                .SelectAwait(async model =>
                {
                    var customerFirstName =
                        await _genericAttributeService.GetAttributeAsync(model.Customer, NopCustomerDefaults.FirstNameAttribute,
                            defaultValue: string.Empty);
                    var customerLastName =
                        await _genericAttributeService.GetAttributeAsync(model.Customer, NopCustomerDefaults.LastNameAttribute,
                            defaultValue: string.Empty);
                    var customerPhoneNumber =
                        await _genericAttributeService.GetAttributeAsync(model.Customer, NopCustomerDefaults.PhoneAttribute,
                            defaultValue: string.Empty);

                    return new TicketsReportExportModel
                    {
                        EventName = await _localizationService.GetLocalizedAsync(model.Product, p => p.Name),
                        EventDate = model.TimeSlot.Date,
                        TimeSlot = model.TimeSlot.StartTime.ToString(),
                        OrderId = model.Order.Id,
                        MyFatoorahReference = model.MyFatoorahReference,

                        TicketPrice = model.Order.OrderTotal,
                        TicketType = model.CashierEvent != null
                            ? "Cashier"
                            : "Online",
                        VisaFee = 0,
                        KNetFee = model.Order.PaymentMethodAdditionalFeeExclTax,
                        ExchangeRate = 1,
                        NetPrice = model.Order.OrderSubtotalExclTax,
                        PaymentType = ClassifyPaymentMethod(model.Order.PaymentMethodSystemName),
                        WalletUsedAmount = model.WalletUsedAmount,
                        UsedPhotosCount = model.ReservationItem.UsedCameraManPhotoCount + model.ReservationItem.UsedCustomerMobilePhotoCount,
                        NotUsedPhotosCount = (model.ReservationItem.CustomerMobilePhotoCount + model.ReservationItem.CameraManPhotoCount) -
                                             (model.ReservationItem.UsedCameraManPhotoCount + model.ReservationItem.UsedCustomerMobilePhotoCount),
                        AccountantName = model.CashierEvent != null
                            ? cashiers.FirstOrDefault(c => c.Id == model.CashierEvent.CustomerId)?.Email
                            : null,
                        ActorsData = model.Actors.Select(a => new TicketsReportActorPartModel
                            {
                                ActorName = a.Name,
                                ActorShare = CalculateShare(model.EventDetail.PhotoPrice, model.ActorEvent.ActorShare,
                                    model.ReservationItem.CameraManPhotoCount + model.ReservationItem.CustomerMobilePhotoCount),
                                ProductionShare = CalculateShare(model.EventDetail.PhotoPrice, model.ActorEvent.ProductionShare,
                                    model.ReservationItem.CameraManPhotoCount + model.ReservationItem.CustomerMobilePhotoCount),
                            })
                            .ToList(),
                        ClientName = customerFirstName + customerLastName,
                        ClientPhoneNumber = customerPhoneNumber,
                        ClientEmail = model.Customer.Email
                    };
                })
                .ToListAsync();
        }
    }
}
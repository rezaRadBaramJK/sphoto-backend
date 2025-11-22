using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Event;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Reports;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Actors;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reports;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Actor;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Production;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.Supervisor;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Reservation;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using ActorData = Nop.Plugin.Baramjk.PhotoPlatform.Models.Reports.ActorData;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Factories
{
    public class ReportsFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly EventFactory _eventFactory;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ISettingService _settingService;


        public ReportsFactory(ILocalizationService localizationService,
            EventFactory eventFactory,
            IPriceFormatter priceFormatter,
            ISettingService settingService)
        {
            _localizationService = localizationService;
            _eventFactory = eventFactory;
            _priceFormatter = priceFormatter;
            _settingService = settingService;
        }

        public async Task<List<ActorReportFiltersDto>> PrepareActorReportFiltersDtoAsync(List<ActorReportDetailsModel> model)
        {
            return await model
                .SelectAwait(async i =>
                {
                    return new ActorReportFiltersDto
                    {
                        EventId = i.EventDetail.EventId,
                        EventName = await _localizationService.GetLocalizedAsync(i.Product, p => p.Name),
                        GroupedTimeSlots = await _eventFactory.PrepareGroupedTimeSlotsDtosAsync(i.TimeSlots, TimeSlotLabelType.Brief)
                    };
                })
                .ToListAsync();
        }


        private List<ActorInfoDto> PrepareActorInfoDtos(List<Actor> model)
        {
            return model
                .Select(x => new ActorInfoDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();
        }


        public async Task<ActorRevenueReportBaseDto> PrepareActorRevenueReportAsync(List<ActorRevenueReportDetailsModel> model)
        {
            var totalRevenue = decimal.Zero;

            var cameraManPhotoCount = 0;
            var customerMobilePhotoCount = 0;
            var actorShare = decimal.Zero;
            model.ForEach(x =>
            {
                totalRevenue += x.ReservationItems.Sum(r =>
                    r.UsedCameraManPhotoCount * x.ActorEvent.CameraManEachPictureCost +
                    r.UsedCustomerMobilePhotoCount * x.ActorEvent.CustomerMobileEachPictureCost);

                actorShare += x.ReservationItems.Sum(r =>
                    r.UsedCameraManPhotoCount * x.ActorEvent.ActorShare +
                    r.UsedCustomerMobilePhotoCount * x.ActorEvent.ActorShare);


                cameraManPhotoCount += x.ReservationItems.Sum(r => r.UsedCameraManPhotoCount);
                customerMobilePhotoCount += x.ReservationItems.Sum(r => r.UsedCustomerMobilePhotoCount);
            });


            return new ActorRevenueReportBaseDto
            {
                TotalRevenue = await _priceFormatter.FormatPriceAsync(totalRevenue),
                CustomerMobilePhotoCount = customerMobilePhotoCount,
                CameraManPhotoCount = cameraManPhotoCount,
                TotalTickets = customerMobilePhotoCount + cameraManPhotoCount,
                ActorShare = await _priceFormatter.FormatPriceAsync(actorShare),
            };
        }

        public async Task<ActorTimeSlotRevenueListModel> PrepareActorTimeSlotRevenueReportAsync(
            List<ActorTimeSlotRevenueReportDetailsModel> model)
        {
            var listModel = new ActorTimeSlotRevenueListModel();

            if (model == null || !model.Any())
                return listModel;

            var groupedByDate = model
                .Where(x => x.ReservationItem.UsedCameraManPhotoCount != 0 || x.ReservationItem.UsedCustomerMobilePhotoCount != 0)
                .GroupBy(x => x.TimeSlot.Date.Date)
                .OrderBy(g => g.Key);

            foreach (var dateGroup in groupedByDate)
            {
                var dateModel = new ActorTimeSlotRevenueDateGroupModel
                {
                    Date = dateGroup.Key.ToString("yyyy-MM-dd")
                };

                var groupedByTimeSlot = dateGroup
                    .GroupBy(x => x.TimeSlot.Id)
                    .OrderBy(g => g.First().TimeSlot.StartTime);

                foreach (var group in groupedByTimeSlot)
                {
                    var groupList = group.ToList();
                    var first = groupList.First();

                    var totalCameraManPhotoCount = groupList.Sum(x => x.ReservationItem.UsedCameraManPhotoCount);
                    var totalCustomerMobilePhotoCount = groupList.Sum(x => x.ReservationItem.UsedCustomerMobilePhotoCount);
                    var totalPhotoPrice = groupList.Sum(x =>
                        (x.ActorEvent.CameraManEachPictureCost * x.ReservationItem.UsedCameraManPhotoCount) +
                        (x.ActorEvent.CustomerMobileEachPictureCost * x.ReservationItem.UsedCustomerMobilePhotoCount));

                    var report = new ActorTimeSlotRevenueItemModel
                    {
                        TotalPhotoCount = totalCameraManPhotoCount + totalCustomerMobilePhotoCount,
                        EventName = await _localizationService.GetLocalizedAsync(first.Product, p => p.Name),
                        TimeSlot = first.TimeSlot.StartTime.ToString(@"hh\:mm"),
                        Date = first.TimeSlot.Date.ToString("d"),
                        TicketPrice = first.ActorEvent.CameraManEachPictureCost * totalCameraManPhotoCount,
                        ActorShare = first.ActorEvent.ActorShare,
                        CameraManPhotoCount = totalCameraManPhotoCount,
                        CustomerMobilePhotoCount = totalCustomerMobilePhotoCount,
                        TotalPhotoPrice = totalPhotoPrice,
                    };

                    dateModel.Items.Add(report);
                    dateModel.TotalDayPhotoCount += report.TotalPhotoCount;
                    dateModel.TotalDayPhotoPrice += totalPhotoPrice;


                    listModel.TotalPhotoCount += report.TotalPhotoCount;
                    listModel.TotalPhotoPrice += totalPhotoPrice;
                }

                listModel.GroupedByDate.Add(dateModel);
            }

            return listModel;
        }


        public async Task<List<ProductionRoleReportFiltersDto>> PrepareReportFiltersAsync(List<EventFullDetails> model)
        {
            return await model
                .SelectAwait(async i =>
                {
                    return new ProductionRoleReportFiltersDto
                    {
                        EventId = i.EventDetail.EventId,
                        EventName = await _localizationService.GetLocalizedAsync(i.Product, p => p.Name),
                        GroupedTimeSlots = await _eventFactory.PrepareGroupedTimeSlotsDtosAsync(i.TimeSlots, TimeSlotLabelType.Brief),
                        Actors = PrepareActorInfoDtos(i.Actors.Select(x => x.Actor).ToList())
                    };
                })
                .ToListAsync();
        }

        public async Task<ProductionRoleReportDto> PrepareProductionRoleReportAsync(List<ReservationDetailsModel> model)
        {
            var settings = await _settingService.LoadSettingAsync<PhotoPlatformSettings>();


            var cameraManPhotoCount = 0;
            var customerMobilePhotoCount = 0;
            var actorShare = decimal.Zero;
            var productionShare = decimal.Zero;
            var totalRevenue = decimal.Zero;


            foreach (var item in model)
            {
                cameraManPhotoCount += item.ReservationItem.UsedCameraManPhotoCount;
                customerMobilePhotoCount += item.ReservationItem.UsedCustomerMobilePhotoCount;
                actorShare += (item.ReservationItem.UsedCameraManPhotoCount + item.ReservationItem.UsedCustomerMobilePhotoCount) *
                              item.ActorEvent.ActorShare;
                productionShare += (item.ReservationItem.UsedCameraManPhotoCount + item.ReservationItem.UsedCustomerMobilePhotoCount) *
                                   item.ActorEvent.ProductionShare;
                totalRevenue +=
                    item.ReservationItem.UsedCameraManPhotoCount * item.ActorEvent.CameraManEachPictureCost +
                    item.ReservationItem.UsedCustomerMobilePhotoCount * item.ActorEvent.CustomerMobileEachPictureCost;
            }


            var groupedByActor = model
                .GroupBy(x => x.Actor.Id)
                .Select(g => new { ActorId = g.Key, Items = g.ToList() });

            var actorsReports = await groupedByActor
                .SelectAwait(async x => new ProductionActorRevenueReportDto
                {
                    ActorName = x.Items.First().Actor.Name,
                    ActorShare = await _priceFormatter.FormatPriceAsync(
                        x.Items.Sum(y =>
                            (y.ReservationItem.UsedCameraManPhotoCount + y.ReservationItem.UsedCustomerMobilePhotoCount) * y.ActorEvent.ActorShare)),
                    ProductionShare = await _priceFormatter.FormatPriceAsync(
                        x.Items.Sum(y =>
                            (y.ReservationItem.UsedCameraManPhotoCount + y.ReservationItem.UsedCustomerMobilePhotoCount) *
                            y.ActorEvent.ProductionShare)),
                    CameraManPhotoCount = x.Items.Sum(y => y.ReservationItem.UsedCameraManPhotoCount),
                    CustomerMobilePhotoCount = x.Items.Sum(y => y.ReservationItem.UsedCustomerMobilePhotoCount),
                    TotalTickets = x.Items.Sum(y => y.ReservationItem.UsedCustomerMobilePhotoCount + y.ReservationItem.UsedCameraManPhotoCount),
                    TotalRevenue = await _priceFormatter.FormatPriceAsync(x.Items.Sum(y =>
                        (y.ReservationItem.UsedCustomerMobilePhotoCount * y.ActorEvent.CustomerMobileEachPictureCost) +
                        (y.ReservationItem.UsedCameraManPhotoCount * y.ActorEvent.CameraManEachPictureCost))),
                })
                .ToListAsync();


            return new ProductionRoleReportDto
            {
                TotalRevenue = await _priceFormatter.FormatPriceAsync(totalRevenue),
                CustomerMobilePhotoCount = customerMobilePhotoCount,
                CameraManPhotoCount = cameraManPhotoCount,
                TotalTickets = customerMobilePhotoCount + cameraManPhotoCount,
                ActorShare = await _priceFormatter.FormatPriceAsync(actorShare),
                ProductionShare = await _priceFormatter.FormatPriceAsync(productionShare),
                TotalActors = model.Select(x => x.Actor.Id).Distinct().Count(),
                ShowTickets = settings.ShowTicketsInProductionReports,
                ActorsReports = actorsReports
            };
        }


        public async Task<SupervisorRevenueReportOverviewDto> PrepareSupervisorReportAsync(bool usedTicketsReportOnly,
            List<ReservationDetailsModel> model)
        {
            var settings = await _settingService.LoadSettingAsync<PhotoPlatformSettings>();


            var cameraManPhotoCount = 0;
            var customerMobilePhotoCount = 0;
            var actorShare = decimal.Zero;
            var productionShare = decimal.Zero;
            var totalPhotoShootShare = decimal.Zero;
            var totalRevenue = decimal.Zero;

            foreach (var item in model)
            {
                if (usedTicketsReportOnly)
                {
                    cameraManPhotoCount += item.ReservationItem.UsedCameraManPhotoCount;
                    customerMobilePhotoCount += item.ReservationItem.UsedCustomerMobilePhotoCount;
                    actorShare += (item.ReservationItem.UsedCameraManPhotoCount + item.ReservationItem.UsedCustomerMobilePhotoCount) *
                                  item.ActorEvent.ActorShare;
                    productionShare += (item.ReservationItem.UsedCameraManPhotoCount + item.ReservationItem.UsedCustomerMobilePhotoCount) *
                                       item.ActorEvent.ProductionShare;
                    totalRevenue +=
                        item.ReservationItem.UsedCameraManPhotoCount * item.ActorEvent.CameraManEachPictureCost +
                        item.ReservationItem.UsedCustomerMobilePhotoCount * item.ActorEvent.CustomerMobileEachPictureCost;
                    totalPhotoShootShare += (item.ReservationItem.UsedCameraManPhotoCount + item.ReservationItem.UsedCustomerMobilePhotoCount) *
                                            item.EventDetail.PhotoShootShare;
                }
                else
                {
                    cameraManPhotoCount += item.ReservationItem.CameraManPhotoCount;
                    customerMobilePhotoCount += item.ReservationItem.CustomerMobilePhotoCount;
                    actorShare += (item.ReservationItem.CameraManPhotoCount + item.ReservationItem.CustomerMobilePhotoCount) *
                                  item.ActorEvent.ActorShare;
                    productionShare += (item.ReservationItem.CameraManPhotoCount + item.ReservationItem.CustomerMobilePhotoCount) *
                                       item.ActorEvent.ProductionShare;
                    totalRevenue +=
                        item.ReservationItem.CameraManPhotoCount * item.ActorEvent.CameraManEachPictureCost +
                        item.ReservationItem.CustomerMobilePhotoCount * item.ActorEvent.CustomerMobileEachPictureCost;

                    totalPhotoShootShare += (item.ReservationItem.CameraManPhotoCount + item.ReservationItem.CustomerMobilePhotoCount) *
                                            item.EventDetail.PhotoShootShare;
                }
            }


            var groupedByActor = model
                .GroupBy(x => x.Actor.Id)
                .Select(g => new { ActorId = g.Key, Items = g.ToList() });


            List<SupervisorActorRevenueReportDto> actorsReports;


            if (usedTicketsReportOnly)
            {
                actorsReports = await groupedByActor
                    .SelectAwait(async x => new SupervisorActorRevenueReportDto
                    {
                        ActorName = x.Items.First().Actor.Name,
                        ActorShare = await _priceFormatter.FormatPriceAsync(
                            x.Items.Sum(y =>
                                (y.ReservationItem.UsedCameraManPhotoCount + y.ReservationItem.UsedCustomerMobilePhotoCount) *
                                y.ActorEvent.ActorShare)),
                        ProductionShare = await _priceFormatter.FormatPriceAsync(
                            x.Items.Sum(y =>
                                (y.ReservationItem.UsedCameraManPhotoCount + y.ReservationItem.UsedCustomerMobilePhotoCount) *
                                y.ActorEvent.ProductionShare)),
                        SPhotoGroupShare = await _priceFormatter.FormatPriceAsync(x.Items.Sum(y =>
                            (y.ReservationItem.UsedCameraManPhotoCount + y.ReservationItem.UsedCustomerMobilePhotoCount) *
                            y.EventDetail.PhotoShootShare)),
                        CameraManPhotoCount = x.Items.Sum(y => y.ReservationItem.UsedCameraManPhotoCount),
                        CustomerMobilePhotoCount = x.Items.Sum(y => y.ReservationItem.UsedCustomerMobilePhotoCount),
                        TotalTickets = x.Items.Sum(y => y.ReservationItem.UsedCustomerMobilePhotoCount + y.ReservationItem.UsedCameraManPhotoCount),
                        TotalRevenue = await _priceFormatter.FormatPriceAsync(x.Items.Sum(y =>
                            (y.ReservationItem.UsedCustomerMobilePhotoCount * y.ActorEvent.CustomerMobileEachPictureCost) +
                            (y.ReservationItem.UsedCameraManPhotoCount * y.ActorEvent.CameraManEachPictureCost))),
                    })
                    .ToListAsync();
            }
            else
            {
                actorsReports = await groupedByActor
                    .SelectAwait(async x => new SupervisorActorRevenueReportDto
                    {
                        ActorName = x.Items.First().Actor.Name,
                        ActorShare = await _priceFormatter.FormatPriceAsync(
                            x.Items.Sum(y =>
                                (y.ReservationItem.CameraManPhotoCount + y.ReservationItem.CustomerMobilePhotoCount) * y.ActorEvent.ActorShare)),
                        ProductionShare = await _priceFormatter.FormatPriceAsync(
                            x.Items.Sum(y =>
                                (y.ReservationItem.CameraManPhotoCount + y.ReservationItem.CustomerMobilePhotoCount) * y.ActorEvent.ProductionShare)),
                        SPhotoGroupShare = await _priceFormatter.FormatPriceAsync(x.Items.Sum(y =>
                            (y.ReservationItem.CameraManPhotoCount + y.ReservationItem.CustomerMobilePhotoCount) * y.EventDetail.PhotoShootShare)),
                        CameraManPhotoCount = x.Items.Sum(y => y.ReservationItem.CameraManPhotoCount),
                        CustomerMobilePhotoCount = x.Items.Sum(y => y.ReservationItem.CustomerMobilePhotoCount),
                        TotalTickets = x.Items.Sum(y => y.ReservationItem.CustomerMobilePhotoCount + y.ReservationItem.CameraManPhotoCount),
                        TotalRevenue = await _priceFormatter.FormatPriceAsync(x.Items.Sum(y =>
                            (y.ReservationItem.CustomerMobilePhotoCount * y.ActorEvent.CustomerMobileEachPictureCost) +
                            (y.ReservationItem.CameraManPhotoCount * y.ActorEvent.CameraManEachPictureCost))),
                    })
                    .ToListAsync();
            }


            return new SupervisorRevenueReportOverviewDto
            {
                TotalRevenue = await _priceFormatter.FormatPriceAsync(totalRevenue),
                CustomerMobilePhotoCount = customerMobilePhotoCount,
                CameraManPhotoCount = cameraManPhotoCount,
                TotalTickets = customerMobilePhotoCount + cameraManPhotoCount,
                ActorShare = await _priceFormatter.FormatPriceAsync(actorShare),
                ProductionShare = await _priceFormatter.FormatPriceAsync(productionShare),
                TotalActors = model.Select(x => x.Actor.Id).Distinct().Count(),
                ShowTickets = settings.ShowTicketsInProductionReports,
                ActorsReports = actorsReports,
                SPhotoGroupShare = await _priceFormatter.FormatPriceAsync(totalPhotoShootShare)
            };
        }

        public async Task<ProductionReportModel> PrepareProductionReportDataAsync(List<ReservationDetailsModel> model)
        {
            var report = new ProductionReportModel();
            var totalCameraManPhotoCount = 0;
            var totalCustomerMobilePhotoCount = 0;

            var groupedByDate = model.GroupBy(r => r.TimeSlot.Date.Date);

            foreach (var dateGroup in groupedByDate)
            {
                var dateModel = new DateGroupedData
                {
                    EventDate = dateGroup.Key,
                    EventName = await _localizationService.GetLocalizedAsync(dateGroup.First().Product, p => p.Name),
                    TimeSlotsData = new List<TimeSlotGroupedData>()
                };

                var groupedByTimeSlot = dateGroup.GroupBy(r => r.TimeSlot.Id);

                foreach (var timeSlotGroup in groupedByTimeSlot)
                {
                    var timeSlotModel = new TimeSlotGroupedData
                    {
                        EventTime = timeSlotGroup.First().TimeSlot.StartTime,
                        ActorsData = new List<ActorData>()
                    };

                    var groupedByActor = timeSlotGroup.GroupBy(r => r.Actor.Id);

                    foreach (var actorGroup in groupedByActor)
                    {
                        var firstInActorGroup = actorGroup.First();

                        var cameraManPhotos = actorGroup.Sum(r => r.ReservationItem.UsedCameraManPhotoCount);
                        var mobilePhotos = actorGroup.Sum(r => r.ReservationItem.UsedCustomerMobilePhotoCount);

                        totalCameraManPhotoCount += cameraManPhotos;
                        totalCustomerMobilePhotoCount += mobilePhotos;

                        var totalProductionPrice = actorGroup.Sum(r =>
                            (r.ActorEvent.ProductionShare * r.ReservationItem.UsedCameraManPhotoCount) +
                            (r.ActorEvent.ProductionShare * r.ReservationItem.UsedCustomerMobilePhotoCount));

                        var actorData = new ActorData
                        {
                            ActorName = firstInActorGroup.Actor.Name,
                            CameraManPhotoCount = cameraManPhotos,
                            CustomerMobilePhotoCount = mobilePhotos,
                            TotalPhotoCount = cameraManPhotos + mobilePhotos,
                            TotalPhotoPrice = totalProductionPrice,
                            TicketPrice = firstInActorGroup.EventDetail.PhotoPrice,
                            ActorShare = firstInActorGroup.ActorEvent.ActorShare,
                            ProductionShare = firstInActorGroup.ActorEvent.ProductionShare,
                        };
                        timeSlotModel.ActorsData.Add(actorData);
                    }

                    timeSlotModel.TotalTimeSlotPhotoCount = timeSlotModel.ActorsData.Sum(ad => ad.TotalPhotoCount);
                    timeSlotModel.TotalTimeSlotPhotoPrice = timeSlotModel.ActorsData.Sum(ad => ad.TotalPhotoPrice);

                    dateModel.TimeSlotsData.Add(timeSlotModel);
                }

                dateModel.TotalDayPhotoCount = dateModel.TimeSlotsData.Sum(ts => ts.TotalTimeSlotPhotoCount);
                dateModel.TotalDayPhotoPrice = dateModel.TimeSlotsData.Sum(ts => ts.TotalTimeSlotPhotoPrice);

                report.GroupedByDate.Add(dateModel);
            }
            var firstItem = model.First();
            
            report.TotalPhotoCount = report.GroupedByDate.Sum(d => d.TotalDayPhotoCount);
            report.TotalPhotoPrice = report.GroupedByDate.Sum(d => d.TotalDayPhotoPrice);
            report.TotalCameraManPhotoCount = totalCameraManPhotoCount;
            report.TotalCustomerMobilePhotoCount = totalCustomerMobilePhotoCount;
            report.GeneralActorShare = firstItem.EventDetail.ActorShare;
            report.GeneralProductionShare = firstItem.EventDetail.ProductionShare;
            report.GeneralPhotoShootShare = firstItem.EventDetail.PhotoShootShare;
            report.GeneralPhotoPrice = firstItem.EventDetail.PhotoPrice;
            return report;
        }

        public async Task<SupervisorReportModel> PrepareSupervisorReportDataAsync(List<ReservationDetailsModel> model,
            bool usedTicketsReportOnly = false, bool showActors = false)
        {
            var report = new SupervisorReportModel();
            var totalCameraManPhotoCount = 0;
            var totalCustomerMobilePhotoCount = 0;

            var groupedByDate = model.GroupBy(r => r.TimeSlot.Date.Date);

            foreach (var dateGroup in groupedByDate)
            {
                var dateModel = new DateGroupedData
                {
                    EventDate = dateGroup.Key,
                    EventName = await _localizationService.GetLocalizedAsync(dateGroup.First().Product, p => p.Name),
                    TimeSlotsData = new List<TimeSlotGroupedData>()
                };

                var groupedByTimeSlot = dateGroup.GroupBy(r => r.TimeSlot.Id);

                foreach (var timeSlotGroup in groupedByTimeSlot)
                {
                    var timeSlotModel = new TimeSlotGroupedData
                    {
                        EventTime = timeSlotGroup.First().TimeSlot.StartTime,
                        ActorsData = new List<ActorData>()
                    };

                    var groupedByActor = timeSlotGroup.GroupBy(r => r.Actor.Id);

                    foreach (var actorGroup in groupedByActor)
                    {
                        var firstInActorGroup = actorGroup.First();

                        int cameraManPhotos;
                        int mobilePhotos;

                        if (usedTicketsReportOnly)
                        {
                            cameraManPhotos = actorGroup.Sum(r => r.ReservationItem.UsedCameraManPhotoCount);
                            mobilePhotos = actorGroup.Sum(r => r.ReservationItem.UsedCustomerMobilePhotoCount);
                        }
                        else
                        {
                            cameraManPhotos = actorGroup.Sum(r => r.ReservationItem.CameraManPhotoCount);
                            mobilePhotos = actorGroup.Sum(r => r.ReservationItem.CustomerMobilePhotoCount);
                        }

                        totalCameraManPhotoCount += cameraManPhotos;
                        totalCustomerMobilePhotoCount += mobilePhotos;

                        var totalPhotoShootPrice = (cameraManPhotos + mobilePhotos) * firstInActorGroup.EventDetail.PhotoShootShare;

                        var actorData = new ActorData
                        {
                            ActorName = firstInActorGroup.Actor.Name,
                            CameraManPhotoCount = cameraManPhotos,
                            CustomerMobilePhotoCount = mobilePhotos,
                            TotalPhotoCount = cameraManPhotos + mobilePhotos,
                            TotalPhotoPrice = totalPhotoShootPrice,
                            TicketPrice = firstInActorGroup.EventDetail.PhotoPrice,
                            ActorShare = firstInActorGroup.ActorEvent.ActorShare,
                            ProductionShare = firstInActorGroup.ActorEvent.ProductionShare,
                        };
                        timeSlotModel.ActorsData.Add(actorData);
                    }

                    timeSlotModel.TotalTimeSlotPhotoCount = timeSlotModel.ActorsData.Sum(ad => ad.TotalPhotoCount);
                    timeSlotModel.TotalTimeSlotPhotoPrice = timeSlotModel.ActorsData.Sum(ad => ad.TotalPhotoPrice);

                    dateModel.TimeSlotsData.Add(timeSlotModel);
                }

                dateModel.TotalDayPhotoCount = dateModel.TimeSlotsData.Sum(ts => ts.TotalTimeSlotPhotoCount);
                dateModel.TotalDayPhotoPrice = dateModel.TimeSlotsData.Sum(ts => ts.TotalTimeSlotPhotoPrice);

                report.GroupedByDate.Add(dateModel);
            }

            var firstItem = model.First();

            report.TotalPhotoCount = report.GroupedByDate.Sum(d => d.TotalDayPhotoCount);
            report.TotalPhotoPrice = report.GroupedByDate.Sum(d => d.TotalDayPhotoPrice);
            report.TotalCameraManPhotoCount = totalCameraManPhotoCount;
            report.TotalCustomerMobilePhotoCount = totalCustomerMobilePhotoCount;
            report.GeneralActorShare = firstItem.EventDetail.ActorShare;
            report.GeneralProductionShare = firstItem.EventDetail.ProductionShare;
            report.GeneralPhotoShootShare = firstItem.EventDetail.PhotoShootShare;
            report.GeneralPhotoPrice = firstItem.EventDetail.PhotoPrice;
            report.ShowActors = showActors;
            report.ReportType = usedTicketsReportOnly
                ? await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.ReportType.UsedTickets")
                : await _localizationService.GetResourceAsync("Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.ReportType.SoledTickets");

            return report;
        }


        public async Task<ActorReportModel> PrepareActorReportModelAsync(List<ReservationDetailsModel> model)
        {
            var dailyData = await model
                .GroupBy(r => r.TimeSlot.Date.Date)
                .SelectAwait(async dateGroup => new DailyEventData
                {
                    EventDate = dateGroup.Key,
                    EventName = await _localizationService.GetLocalizedAsync(dateGroup.First().Product, p => p.Name),
                    TimeSlots = await dateGroup
                        .GroupBy(r => r.TimeSlot.Id)
                        .SelectAwait(async timeSlotGroup => new TimeSlotData
                        {
                            TimeSlotId = timeSlotGroup.Key,
                            StartTime = timeSlotGroup.First().TimeSlot.StartTime,
                            ActorsData = await timeSlotGroup
                                .GroupBy(r => r.Actor.Id)
                                .SelectAwait(async actorGroup =>
                                {
                                    var actor = actorGroup.First().Actor;
                                    return new ActorData()
                                    {
                                        ActorName = await _localizationService.GetLocalizedAsync(actor, a => a.Name),


                                        TotalPhotoCount = actorGroup.Sum(r =>
                                            r.ReservationItem.UsedCameraManPhotoCount +
                                            r.ReservationItem.UsedCustomerMobilePhotoCount)
                                    };
                                })
                                .ToListAsync()
                        })
                        .ToListAsync()
                })
                .ToListAsync();

            var settings = await _settingService.LoadSettingAsync<PhotoPlatformSettings>();

            return new ActorReportModel
            {
                GroupedByDate = dailyData,
                ShowTickets = settings.ShowTicketsInProductionReports,
            };
        }
    }
}
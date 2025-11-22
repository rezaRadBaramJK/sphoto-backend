using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Plugins
{
    public class PhotoPlatformPlugin : BaramjkPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;
        private readonly PhotoPlatformCustomerService _photoPlatformCustomerService;


        public PhotoPlatformPlugin(
            ILocalizationService localizationService,
            IPermissionService permissionService,
            WidgetSettings widgetSettings,
            ISettingService settingService,
            PhotoPlatformCustomerService photoPlatformCustomerService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _widgetSettings = widgetSettings;
            _settingService = settingService;
            _photoPlatformCustomerService = photoPlatformCustomerService;
        }

        public override bool AutoAddConfigurationMenu => true;

        public override Task InstallAsync()
        {
            return Task.WhenAll(
                _localizationService.AddLocaleResourceAsync(Localizations),
                _permissionService.InstallPermissionsAsync(new PermissionProvider()),
                ActiveWidgetsAsync(),
                _photoPlatformCustomerService.SubmitScheduleTaskAsync()
            );
        }

        private async Task ActiveWidgetsAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(DefaultValues.SystemName))
                return;

            _widgetSettings.ActiveWidgetSystemNames.Add(DefaultValues.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }


        public override Task UninstallAsync()
        {
            return Task.WhenAll(
                _localizationService.DeleteLocaleResourcesAsync("Nop.Plugin.Baramjk.PhotoPlatform"),
                _photoPlatformCustomerService.DeleteScheduleTaskAsync()
            );
        }

        private static SiteMapNode _cachePluginSiteMapNode;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await AuthorizeAsync(PermissionProvider.ManagementRecord) == false)
                return;

            if (_cachePluginSiteMapNode == null)
            {
                var nodes = new List<SiteMapNode>
                {
                    CreateSiteMapNode(
                        controllerName: "Actor",
                        actionName: "List",
                        title: "Actors",
                        systemName: $"{SystemName}.Actors"),
                    new()
                    {
                        Title = "Reports",
                        Visible = true,
                        IconClass = MenuUtils.IconClassSubsItem,
                        ChildNodes = new List<SiteMapNode>
                        {
                            CreateSiteMapNode(
                                controllerName: "Report",
                                actionName: "CashierReport",
                                title: "Cashier Report",
                                systemName: $"{SystemName}.CashierReport"),
                            CreateSiteMapNode(
                                controllerName: "Report",
                                actionName: "SalesReport",
                                title: "Sales Report",
                                systemName: $"{SystemName}.SalesReport"),
                            CreateSiteMapNode(
                                controllerName: "Report",
                                actionName: "CashierEventReport",
                                title: "Cashier Event Report",
                                systemName: $"{SystemName}.CashierEventReport"),
                            CreateSiteMapNode(
                                controllerName: "Report",
                                actionName: "TicketsReport",
                                title: "Ticket Report",
                                systemName: $"{SystemName}.TicketsReport"),
                        }
                    },
                    new()
                    {
                        Title = "Contact Us",
                        Visible = true,
                        IconClass = MenuUtils.IconClassSubsItem,
                        ChildNodes = new List<SiteMapNode>
                        {
                            CreateSiteMapNode(
                                "ContactUs",
                                "Configure",
                                "Configure",
                                $"{SystemName}.ContactUs.Configure"),
                            CreateSiteMapNode(
                                "ContactUsSubject",
                                "List",
                                "Subjects",
                                $"{SystemName}.ContactUsSubjects.List"),
                            CreateSiteMapNode(
                                "ContactUsInfo",
                                "List",
                                "Contact Infos",
                                $"{SystemName}.ContactUsInfos.List")
                        }
                    }
                };


                _cachePluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, nodes.ToArray());
            }

            rootNode.AddToBaramjkPluginsMenu(_cachePluginSiteMapNode);
        }

        public bool HideInWidgetList => false;

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.ProductDetailsBlock,
                AdminWidgetZones.CustomerDetailsBlock,
            });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "PhotoPlatformAdminViewComponent";
        }

        public static Dictionary<string, string> Localizations => new()
        {
            //admin
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details", "Event Details" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.TimeSlots", "Time Slots" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.TermsAndConditions", "Terms And Conditions" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.StartDate", "Start Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.EndDate", "End Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.StartTime", "Start Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.EndTime", "End Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.TimeSlotDuration", "Time Slot Duration (Minutes)" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.Save", "Save" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Widget.ActorEvent.Title", "Actors" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Widget.TimeSlot.Title", "Times" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Widget.Actor.Title", "Associated Actor" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.ProductionShare", "Production Share" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.ActorShare", "Actor Share" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.PhotoPrice", "Photo Price" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.PhotoShootShare", "PhotoShoot Share" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.TimeSlotLabelTypeId", "Time Slot Label Type" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.CountryId", "Country" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.CountryIds", "Countries" },

            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.Note", "Note" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.LocationUrl", "Location Url" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Event.Details.LocationUrlTitle", "Location Url Title" },
            //admin - timeSlots
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.CreateNewTitle", "Create New Time Slot" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.EditTimeSlot", "Edit Time Slot" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.BackToEventDetails", "Back To Event Details" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Date", "Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.StartTime", "Start Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.EndTime", "End Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Active", "Active" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.TimeSlot.Note", "Note" },

            //admin - actors
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor", "Actors" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.CreateNewActor", "Create New Actor" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.EditActor", "Edit Actor" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.BackToActorsList", "Back To Actors List" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Name", "Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.CardNumber", "Card Number" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.CardHolderName", "Card Holder Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Picture", "Picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.DefaultCameraManEachPictureCost", "CameraMan Each Picture Cost" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.DefaultCustomerMobileEachPictureCost", "Customer Mobile Each Picture Cost" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Email", "Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Password", "Password" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.CustomerId", "Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Pictures", "Pictures" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Pictures.SaveBeforeEdit", "You should save the actor first, then you can add pictures" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Pictures.Header.AddNew", "Add new picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Pictures.Button.AddNew", "Add actor picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Pictures.Alert.AddNew", "Upload picture first" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Actor.Pictures.Alert.PictureAdd", "Failed to add actor picture." },

            //admin - actors - search
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorSearchModel.SearchEmail", "Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorSearchModel.SearchName", "Name" },

            //admin - actors - pictures
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorPicture.ActorId", "Actor Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorPicture.PictureId", "Picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorPicture.PictureUrl", "Picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorPicture.DisplayOrder", "Display Order" },


            //admin - actorEvents
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.EventId", "Event Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorId", "Actor Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorName", "Actor Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorPictureUrl", "Actor Picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.CameraManEachPictureCost", "CameraMan Each Picture Cost" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.CustomerMobileEachPictureCost", "Customer Mobile Each Picture Cost" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.CommissionAmount", "Commission Amount" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorPhotoPrice", "Actor Photo Price" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.DisplayOrder", "Display Order" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ActorShare", "Actor Share" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEvents.ProductionShare", "Production Share" },


            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.EditAllProductionShares", "Edit Production Shares" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.EditAllActorShares", "Edit Actor Shares" },

            //admin - actorEventTimeSlots

            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEventTimeSlots.Active", "Active" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEventTimeSlots.Title", "Actors Availability" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ActorEventTimeSlots.ViewActors", "View Actors" },
            //admin - actorEvents - CustomerDetailsWidget
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CustomerDetailsWidget.ActorId", "Actor Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CustomerDetailsWidget.CustomerId", "Customer Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CustomerDetailsWidget.ActorLink", "Actor Link" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CustomerDetailsWidget.NotAssociatedToActor", "This Customer is not associated to any actor" },


            //admin - cashierEvents
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Widget.CashierEvent.Title", "Cashiers" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvent", "Cashiers" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvent.Balances", "Cashiers Balances" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvent.DailyBalance", "Balance" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.EventId", "Event Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CustomerId", "Customer Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CashierEmail", "Cashier Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.Cashier", "Cashier" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CashierPictureUrl", "Cashier Picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.CommissionAmount", "Commission Amount" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.OpeningFundBalanceAmount", "Opening Fund Balance Amount" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.Day", "Day" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.Active", "Active" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.IsRefundPermitted", "Refund Permitted" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.CashierEvents.ChangeBalance", "Change Balance" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ChangeCashierBalanceViewModel.ChangeAmount", "Change Amount" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ChangeCashierBalanceViewModel.ChangeType", "Change Type" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ChangeCashierBalanceViewModel.Title", "Change Cashier Balance" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Cashier", "Cashier" },

            //admin - supervisorEvents
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Widget.SupervisorEvent.Title", "Supervisors" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Supervisor", "Supervisor" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvent", "Supervisors" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.EventId", "Event Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.CustomerId", "Customer Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.SupervisorEmail", "Supervisor Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.SupervisorPictureUrl", "Supervisor Picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.SupervisorEvents.Active", "Active" },


            //admin - productionEvents
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Widget.ProductionEvents.Title", "Productions" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Widget.ProductionEvents.title", "Productions" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.EventId", "Event Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.CustomerId", "Customer Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.ProductionEmail", "Production Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.ProductionPictureUrl", "Production Picture" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.ProductionEvents.Active", "Active" },


            //admin - reports - cashier
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReportTitle", "Cashier Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.GenerateReportButton", "Generate Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.EventId", "Event" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.EventDate", "Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.TimeSlotId", "Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierReport.ShowTicketsCount", "Show Tickets Count" },

            //admin - reports - sales
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Sales.EventId", "Event" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Sales.Title", "Sales Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Sales.GenerateReportButton", "Generate Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Sales.OnlyShowConfirmedTickets", "Only Show Confirmed Tickets" },

            //admin - reports - cashierEvent
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.Title", "Event Cashier Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.GenerateReportButton", "Generate Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.CashierEventReportViewModel.EventId", "Event" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.CashierEventReportViewModel.CashierId", "Cashier" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.CashierEventReportViewModel.EventDate", "Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.CashierEvent.CashierEventReportViewModel.TimeSlotId", "Time" },

            //
            { "Nop.Plugin.Baramjk.PhotoPlatform.Api.GroupedTimeSlots.DayLabel", "الیوم" },


            //admin - reports - ticket reports
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.Title", "Tickets Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.EventIds", "Events" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.EventDate", "Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.FromDate", "From Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.ToDate", "To Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.TimeSlotId", "Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.TicketsReportViewModel.TimeSlotStartTime", "Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.TicketsReport.ExportToExcel", "Export to Excel" },

            //admin - pdf report - cashier
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierReport.Title", "فنانين مطلوبين للتصوير" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierReport.Day", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierReport.Row", "المسلسل" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierReport.TimeSlot", "رقم العرض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierReport.EventName", "المسرحیه" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierReport.ActorName", "الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierReport.VerifiedPhotosCount", "العدد" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierReport.PrintDate", "تاريخ الطباعه" },

            //admin - pdf report - sales
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.Title", "تقرير توزيع المبيعات" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.EventName", "المسرحية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.Day", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.ActorName", "الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.NumberOfTickets", "عدد التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.UnitPrice", "سعر الوحدة" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.TotalPrice", "الإجمالي" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.Total", "الإجمالي" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.ProductionShare", "نسبة الإنتاج" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.ActorShare", "نسبة الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.PhotoShootShare", "نسبة التصویر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.Summary", "ملخص التقرير" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.SumNumberOfTickets", "اجمالي عدد التذاكر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.SumTotalPrices", "اجمالي قيمة التذاكر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.SumTotalProductionShare", "نسبة الانتاج" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.SumTotalActorShare", "نسبة الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.Sales.SumTotalPhotoShootShare", "نسبة التصویر" },

            //admin - pdf report - cashier event 
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.Title", "تقرير إغلاق اليوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.Day", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.TimeSlot", "رقم العرض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.EventName", "المسرحية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.PrintedDate", "التاريخ" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.CashierName", "المحاسب" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.TableTitle", "البند" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.NumberOfTickets", "العدد" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.PriceKD", "المبلغ د.ك" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.CameraPhotoData", "تحميض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.CustomerMobilePhotoData", "موبايل" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.TotalPhotoData", "الاجمالي" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.RefundPhotoData", "المرتجع" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.NetPhotoData", "الصافي" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.OpeningFundBalance", "الرصيد الإفتتاحي للصندوق" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.CashPayments", "المبيعات النقدية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.TotalFundBalance", "اجمالي الصندوق" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.KNetPayments", "المبيعات كي نت" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.OnlinePayments", "المبيعات أونلاين" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Pdf.CashierEvent.NetProfit", "صافي المبيعات" },

            //admin - tickets report
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.EventName", "Event Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.EventDate", "Date" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.Time", "Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.OrderId", "Order ID" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.MyFatoorahRef", "MyFatoorah Ref" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.TicketPrice", "Ticket Price (KWD)" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.TicketType", "Ticket Type" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.VisaFees", "Visa Fees" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.KNetFees", "KNet Fees" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ExchangeRate", "Exchange Rate" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.NetPrice", "Net Price (KWD)" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.PaymentType", "Payment Type" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.WalletUsed", "Wallet Used" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.PhotosNotUsed", "Photos Not Used" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.PhotosUsed", "Photos Used" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.AccountantName", "Accountant Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ActorName", "Actor Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ActorShare", "Actor Share (%)" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ProductionShare", "Production Share (%)" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ClientName", "Client Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ClientPhone", "Client Phone" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ClientEmail", "Client Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.Report.Excel.TicketsReport.ReportName", "Tickets Report" },

            //admin - contact us
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Settings.PaymentCallbackUrl", "Payment Callback Url" },

            //admin - contact us - subjects
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.Title", "Subjects" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.AddSubject", "Add New Subject" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.EditSubject", "Edit Subject" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.Name", "Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.BackToSubjectsList", "Back to subjects list" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.IsPayable", "Is Payable" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.Price", "Price" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.NotifyAdminAfterPayment", "Notify Admin After Payment" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.NotifyAdminEmail", "Notify Admin Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.OwnerAdminEmail", "Owner Admin Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.Subjects.OwnerPhoneNumber", "Owner Phone Number" },

            //admin - contact us - Contact info
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Title", "Contact Infos" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Edit", "Edit Contact Info" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.BackToContactInfoList", "Back To Contact Info List" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.FirstName", "First Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.LastName", "Last Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.FullName", "Full Name" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Country", "Country" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.PhoneNumber", "PhoneNumber" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Email", "Email" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Subject", "Subject" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.TicketId", "Ticket Id" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.File", "FileId" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.Message", "Message" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.HasBeenPaid", "Has Been Paid" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.SubjectPrice", "Subject Price" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.PaymentDateTime", "Payment Date Time" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.PaymentStatus", "Payment Status" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.PaymentStatus.NoNeedToPay", "No Need To Pay" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.PaymentStatus.Paid", "Paid" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ContactUs.Admin.ContactInfos.PaymentStatus.HasNotPaid", "Has not paid" },


            //actor report
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.EventName", "اسم المسرحية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.Date", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.TimeSlot", "العرض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.TicketPrice", "سعر التذکرة" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.ActorShare", "نسبة الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.CameraManPhotoCount", "عدد تذاکر الهاتف" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.CustomerMobilePhotoCount", "عدد تذاکر الکامیرا" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.TotalPhotoCount", "اجمالي عدد التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.TotalPhotosPrice", "اجمالی قیمة التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Pdf.ActorTimeSlotRevenue.Title", "Actor TimeSlot Revenue Report" },

            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.EventName", "اسم المسرحية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.Date", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TimeSlot", "العرض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TicketPrice", "سعر التذکرة" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.ActorShare", "نسبة الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.CameraManPhotoCount", "عدد تذاکر الهاتف" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.CustomerMobilePhotoCount", "عدد تذاکر الکامیرا" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPhotoCount", "اجمالي عدد التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPhotosPrice", "اجمالی قیمة التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.GrandTotal", "Grand Total (All Days)" },

            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.ReportName", "Actor Revenue Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalTickets", "Total Tickets" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalPrice", "Total Price" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.Total", "الإجمالي" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.Summary", "ملخص التقرير" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalCustomerMobilePhotoCount", "Total Mobile Tickets" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Actor.Report.Excel.TotalCameraManPhotoCount", "Total Camera Tickets" },


            //production report
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.EventName", "اسم المسرحية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.ActorName", "اسم الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.Date", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.TimeSlot", "العرض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.TicketPrice", "سعر التذکرة" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.ActorShare", "نسبة الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.ProductionShare", "نسبة الإنتاج" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.CameraManPhotoCount", "عدد تذاکر الهاتف" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.CustomerMobilePhotoCount", "عدد تذاکر الکامیرا" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.TotalPhotoCount", "اجمالي عدد التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.TotalPhotosPrice", "اجمالی قیمة التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.Total", "المجموع" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.RowNumber", "رقم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Pdf.Title", "Production Report" },

            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.EventName", "اسم المسرحية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.ActorName", "اسم الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Date", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TimeSlot", "العرض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TicketPrice", "سعر التذکرة" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.ActorShare", "نسبة الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.ProductionShare", "نسبة الإنتاج" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.CameraManPhotoCount", "عدد تذاکر الهاتف" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.CustomerMobilePhotoCount", "عدد تذاکر الکامیرا" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalPhotoCount", "اجمالي عدد التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalPhotosPrice", "اجمالی قیمة التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Total", "المجموع" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.RowNumber", "رقم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Title", "Production Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.GrandTotal", "Grand Total (All Days)" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.Summary", "Summary" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalCustomerMobilePhotoCount", "Total Mobile Tickets" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalCameraManPhotoCount", "Total Camera Tickets" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalTickets", "Total Tickets" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Production.Report.Excel.TotalPrice", "Total Price" },


            //supervisor report 
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.EventName", "اسم المسرحية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.ActorName", "اسم الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.Date", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.TimeSlot", "العرض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.TicketPrice", "سعر التذکرة" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.ActorShare", "نسبة الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.PhotoShootShare", "نسبة التصویر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.ProductionShare", "نسبة الإنتاج" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.CameraManPhotoCount", "عدد تذاکر الهاتف" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.CustomerMobilePhotoCount", "عدد تذاکر الکامیرا" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.TotalPhotoCount", "اجمالي عدد التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.TotalPhotosPrice", "اجمالی قیمة التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.Total", "المجموع" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.ReportType.SoledTickets", "التذاکر المباعة" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.ReportType.UsedTickets", "التذاکر المستخدمة" },

            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.EventName", "اسم المسرحية" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.ActorName", "اسم الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.Date", "الیوم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TimeSlot", "العرض" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TicketPrice", "سعر التذکرة" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.ActorShare", "نسبة الفنان" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.ProductionShare", "نسبة الإنتاج" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.CameraManPhotoCount", "عدد تذاکر الهاتف" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.CustomerMobilePhotoCount", "عدد تذاکر الکامیرا" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalPhotoCount", "اجمالي عدد التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalPhotosPrice", "اجمالی قیمة التذاکر" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.Total", "المجموع" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.RowNumber", "رقم" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.Title", "Production Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.GrandTotal", "Grand Total (All Days)" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalCustomerMobilePhotoCount", "Total Mobile Tickets" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalCameraManPhotoCount", "Total Camera Tickets" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalTickets", "Total Tickets" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.TotalPrice", "Total Price" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Excel.ReportType", "نوع التقریر" },


            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.Title", "Production Report" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Supervisor.Report.Pdf.ReportType", "نوع التقریر" },
            //admin - configuration
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.TimeSlotLabelTypeId", "TimeSlot label type" },
            {
                "Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.ShowTicketsInProductionReports",
                "Show Tickets In Production Reports"
            },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.OrderDetailsFrontendBaseUrl", "Order Details FrontEnd base" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.TicketSmsMessage", "Ticket Sms Message" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.Admin.PhotoPlatformSettingsModel.BirthDayRewardPoints", "Birthday Reward Points" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ScannerBoy.PushNotification.Title", "New PhotoShoot!" },
            { "Nop.Plugin.Baramjk.PhotoPlatform.ScannerBoy.PushNotification.Description", "You have a new photoshoot!" }
        };
    }
}
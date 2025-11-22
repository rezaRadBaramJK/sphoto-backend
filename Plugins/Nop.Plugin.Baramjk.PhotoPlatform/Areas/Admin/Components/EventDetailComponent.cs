using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ProductionEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.SupervisorEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Components
{
    [ViewComponent(Name = "PhotoPlatformAdminViewComponent")]
    public class EventDetailComponent : BaramjkViewComponent
    {
        private readonly EventDetailsAdminFactory _eventDetailsAdminFactory;
        private readonly EventDetailsService _eventDetailsService;
        private readonly TimeSlotAdminFactory _timeSlotAdminFactory;
        private readonly ActorEventAdminFactory _actorEventAdminFactory;
        private readonly ActorAdminFactory _actorAdminFactory;
        private readonly CashierEventAdminFactory _cashierEventAdminFactory;
        private readonly SupervisorEventAdminFactory _supervisorEventAdminFactory;
        private readonly ProductionEventAdminFactory _productionEventAdminFactory;

        public EventDetailComponent(
            EventDetailsAdminFactory eventDetailsAdminFactory,
            EventDetailsService eventDetailsService,
            TimeSlotAdminFactory timeSlotAdminFactory,
            ActorEventAdminFactory actorEventAdminFactory,
            ActorAdminFactory actorAdminFactory,
            CashierEventAdminFactory cashierEventAdminFactory,
            SupervisorEventAdminFactory supervisorEventAdminFactory,
            ProductionEventAdminFactory productionEventAdminFactory)
        {
            _eventDetailsAdminFactory = eventDetailsAdminFactory;
            _eventDetailsService = eventDetailsService;
            _timeSlotAdminFactory = timeSlotAdminFactory;
            _actorEventAdminFactory = actorEventAdminFactory;
            _actorAdminFactory = actorAdminFactory;
            _cashierEventAdminFactory = cashierEventAdminFactory;
            _supervisorEventAdminFactory = supervisorEventAdminFactory;
            _productionEventAdminFactory = productionEventAdminFactory;
        }


        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
                return await InvokeProductDetailsBlockAsync(additionalData);

            if (widgetZone == AdminWidgetZones.CustomerDetailsBlock)
                return await InvokeCustomerDetailsBlockAsync(additionalData);


            return Content(string.Empty);
        }

        private async Task<IViewComponentResult> InvokeProductDetailsBlockAsync(object additionalData)
        {
            if (additionalData is not ProductModel productDetailsModel ||
                productDetailsModel.Id == 0)
                return Content(string.Empty);

            var model = new ProductDetailsWidgetViewModel
            {
                EventDetails = await PrepareDetailsViewModelAsync(productDetailsModel.Id),
                TimeSlots = PrepareTimeSlotsViewModel(productDetailsModel.Id),
                ActorEvents = PrepareActorEventsViewModel(productDetailsModel.Id),
                CashierEvents = PrepareCashierEventsViewModel(productDetailsModel.Id),
                ProductionEvents = PrepareProductionEventsViewModel(productDetailsModel.Id),
                EventCashiersDailyBalances = PrepareCashierDailyBalanceSearchModel(productDetailsModel.Id),
                SupervisorEvents = PrepareSupervisorEventsViewModel(productDetailsModel.Id)
            };

            return ViewBase("~/Plugins/Baramjk.PhotoPlatform/Areas/Admin/Views/Widgets/ProductDetailsWidgets.cshtml", model);
        }

        private async Task<EventDetailsViewModel> PrepareDetailsViewModelAsync(int eventId)
        {
            var eventDetails = await _eventDetailsService.GetByEventIdAsync(eventId);
            var eventCountries = await _eventDetailsService.GetEventCountriesAsync(eventId);
            return await _eventDetailsAdminFactory.PrepareDetailsViewModelAsync(eventDetails, eventId, eventCountries);
        }

        private EventTimeSlotsViewModel PrepareTimeSlotsViewModel(int eventId)
        {
            var viewModel = _timeSlotAdminFactory.PrepareEventTimeSlotsViewModel(eventId);
            return viewModel;
        }

        private EventActorEventsViewModel PrepareActorEventsViewModel(int eventId)
        {
            var viewModel = _actorEventAdminFactory.PrepareEventActorEventsViewModel(eventId);
            return viewModel;
        }


        private EventCashierEventsViewModel PrepareCashierEventsViewModel(int eventId)
        {
            var viewModel = _cashierEventAdminFactory.PrepareEventCashierEventsViewModel(eventId);
            return viewModel;
        }

        private CashierDailyBalanceSearchModel PrepareCashierDailyBalanceSearchModel(int eventId)
        {
            var viewModel = _cashierEventAdminFactory.PrepareEventCashierDailyBalanceSearchModel(eventId);
            return viewModel;
        }

        private EventSupervisorEventsViewModel PrepareSupervisorEventsViewModel(int eventId)
        {
            var viewModel = _supervisorEventAdminFactory.PrepareEventSupervisorEventsViewModel(eventId);
            return viewModel;
        }

        private EventProductionEventsViewModel PrepareProductionEventsViewModel(int eventId)
        {
            var viewModel = _productionEventAdminFactory.PrepareEventProductionEventsViewModel(eventId);
            return viewModel;
        }


        private async Task<IViewComponentResult> InvokeCustomerDetailsBlockAsync(object additionalData)
        {
            if (additionalData is not CustomerModel customerModel ||
                customerModel.Id == 0)
                return Content(string.Empty);


            var model = await _actorAdminFactory.PrepareCustomerDetailsWidgetModelAsync(customerModel.Id);
            return ViewBase("~/Plugins/Baramjk.PhotoPlatform/Areas/Admin/Views/Widgets/CustomerDetailsWidget.cshtml", model);
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.SupervisorEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class SupervisorEventAdminFactory
    {
        private readonly SupervisorEventService _supervisorEventService;
        private readonly ICustomerService _customerService;
        private readonly IPictureService _pictureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly MediaSettings _mediaSettings;
        private readonly CustomerSettings _customerSettings;

        public SupervisorEventAdminFactory(SupervisorEventService supervisorEventService, 
            ICustomerService customerService,
            IPictureService pictureService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings,
            CustomerSettings customerSettings)
        {
            _supervisorEventService = supervisorEventService;
            _customerService = customerService;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _mediaSettings = mediaSettings;
            _customerSettings = customerSettings;
        }
        
        public EventSupervisorEventsViewModel PrepareEventSupervisorEventsViewModel(int eventId)
        {
            var viewModel = new EventSupervisorEventsViewModel()
            {
                EventId = eventId,
            };
            viewModel.SetGridPageSize();

            return viewModel;
        }
        
        public async Task<CustomerListModel> PrepareSupervisorListModelAsync(int eventId, CustomerSearchModel searchModel)
        {
            var supervisorRole = await _customerService.GetCustomerRoleBySystemNameAsync(DefaultValues.SupervisorRoleName);
            var supervisors = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { supervisorRole.Id }, pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);


            var eventSupervisors = await _supervisorEventService.GetEventSupervisorsAsync(eventId);

            var notAssociatedSupervisors = supervisors.Where(x => eventSupervisors.Select(ce => ce.CustomerId).Contains(x.Id) == false).ToArray().ToPagedList(searchModel);

            return await new CustomerListModel().PrepareToGridAsync(searchModel, notAssociatedSupervisors, () =>
            {
                return notAssociatedSupervisors.SelectAwait(async cashier => new CustomerModel()
                {
                    Id = cashier.Id,
                    FullName = await _customerService.GetCustomerFullNameAsync(cashier),
                    Email = cashier.Email,
                });
            });
        }
        public async Task<SupervisorEventsListModel> PrepareEventSupervisorEventsListModelAsync(EventSupervisorEventsViewModel viewModel)
        {
            var pageIndex = viewModel.Page - 1;
            var supervisorEvents = await _supervisorEventService.GetAllEventSupervisorEventsAsync(viewModel.EventId, pageIndex, viewModel.PageSize);

            return await new SupervisorEventsListModel().PrepareToGridAsync(viewModel, supervisorEvents, () =>
            {
                return supervisorEvents.SelectAwait(async supervisorEvent =>
                {
                    var customer = await _customerService.GetCustomerByIdAsync(supervisorEvent.CustomerId);
                    var avatarPictureId =
                        await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute);
                    return new SupervisorEventsItemModel()
                    {
                        Id = supervisorEvent.Id,
                        EventId = viewModel.EventId,
                        CustomerId = customer.Id,
                        SupervisorEmail = customer.Email,
                        SupervisorPictureUrl = await _pictureService
                            .GetPictureUrlAsync(avatarPictureId, _mediaSettings.CartThumbPictureSize, _customerSettings.DefaultAvatarEnabled,
                                defaultPictureType: PictureType.Avatar),
                        Active = supervisorEvent.Active,
                    };
                });
            });
        }
    }
}
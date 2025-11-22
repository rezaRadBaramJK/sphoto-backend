using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ProductionEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class ProductionEventAdminFactory
    {
        private readonly ProductionEventService _productionEventService;
        private readonly ICustomerService _customerService;
        private readonly IPictureService _pictureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly MediaSettings _mediaSettings;
        private readonly CustomerSettings _customerSettings;

        public ProductionEventAdminFactory(ProductionEventService productionEventService, 
            ICustomerService customerService,
            IPictureService pictureService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings,
            CustomerSettings customerSettings)
        {
            _productionEventService = productionEventService;
            _customerService = customerService;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _mediaSettings = mediaSettings;
            _customerSettings = customerSettings;
        }
        
        public EventProductionEventsViewModel PrepareEventProductionEventsViewModel(int eventId)
        {
            var viewModel = new EventProductionEventsViewModel()
            {
                EventId = eventId,
            };
            viewModel.SetGridPageSize();

            return viewModel;
        }
        
        public async Task<CustomerListModel> PrepareProductionListModelAsync(int eventId, CustomerSearchModel searchModel)
        {
            var productionRole = await _customerService.GetCustomerRoleBySystemNameAsync(DefaultValues.ProductionRoleName);
            var productions = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { productionRole.Id }, pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);


            var eventProductions = await _productionEventService.GetEventProductionsAsync(eventId);

            var notAssociatedProductions = productions.Where(x => eventProductions.Select(ce => ce.CustomerId).Contains(x.Id) == false).ToArray().ToPagedList(searchModel);

            return await new CustomerListModel().PrepareToGridAsync(searchModel, notAssociatedProductions, () =>
            {
                return notAssociatedProductions.SelectAwait(async cashier => new CustomerModel()
                {
                    Id = cashier.Id,
                    FullName = await _customerService.GetCustomerFullNameAsync(cashier),
                    Email = cashier.Email,
                });
            });
        }
        public async Task<ProductionEventsListModel> PrepareEventProductionEventsListModelAsync(EventProductionEventsViewModel viewModel)
        {
            var pageIndex = viewModel.Page - 1;
            var productionEvents = await _productionEventService.GetAllEventProductionEventsAsync(viewModel.EventId, pageIndex, viewModel.PageSize);

            return await new ProductionEventsListModel().PrepareToGridAsync(viewModel, productionEvents, () =>
            {
                return productionEvents.SelectAwait(async productionEvent =>
                {
                    var customer = await _customerService.GetCustomerByIdAsync(productionEvent.CustomerId);
                    var avatarPictureId =
                        await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute);
                    return new ProductionEventsItemModel()
                    {
                        Id = productionEvent.Id,
                        EventId = viewModel.EventId,
                        CustomerId = customer.Id,
                        ProductionEmail = customer.Email,
                        ProductionPictureUrl = await _pictureService
                            .GetPictureUrlAsync(avatarPictureId, _mediaSettings.CartThumbPictureSize, _customerSettings.DefaultAvatarEnabled,
                                defaultPictureType: PictureType.Avatar),
                        Active = productionEvent.Active,
                    };
                });
            });
        }
    }
}
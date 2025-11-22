using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.CashierEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class CashierEventAdminFactory
    {
        private readonly CashierEventService _cashierEventService;
        private readonly ICustomerService _customerService;
        private readonly IPictureService _pictureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly MediaSettings _mediaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CashierBalanceService _cashierBalanceService;

        public CashierEventAdminFactory(CashierEventService cashierEventService,
            ICustomerService customerService,
            IPictureService pictureService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings,
            CustomerSettings customerSettings,
            CashierBalanceService cashierBalanceService)
        {
            _cashierEventService = cashierEventService;
            _customerService = customerService;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _mediaSettings = mediaSettings;
            _customerSettings = customerSettings;
            _cashierBalanceService = cashierBalanceService;
        }

        public EventCashierEventsViewModel PrepareEventCashierEventsViewModel(int eventId)
        {
            var viewModel = new EventCashierEventsViewModel()
            {
                EventId = eventId,
            };
            viewModel.SetGridPageSize();

            return viewModel;
        }

        public CashierDailyBalanceSearchModel PrepareEventCashierDailyBalanceSearchModel(int eventId)
        {
            var searchModel = new CashierDailyBalanceSearchModel()
            {
                EventId = eventId,
            };
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public async Task<CashierDailyBalanceViewModel> PrepareEventCashierDailyBalanceViewModel(int eventId, CashierDailyBalanceDetails model = null)

        {
            var eventCashiers = await _cashierEventService.GetEventCashiersCustomerDataAsync(eventId);
            var viewModel = new CashierDailyBalanceViewModel()
            {
                EventId = eventId,
                AvailableCashiers = eventCashiers
                    .Select(c => new SelectListItem()
                    {
                        Text = c.Email,
                        Value = c.Id.ToString(),
                    })
                    .ToList()
            };

            if (model == null)
            {
                return viewModel;
            }

            viewModel.CashierEmail = model.Customer.Email;
            viewModel.CashierId = model.Customer.Id;
            viewModel.OpeningFundBalanceAmount = model.CashierDailyBalance.OpeningFundBalanceAmount;
            viewModel.Day = model.CashierDailyBalance.Day;
            return viewModel;
        }

        public async Task<CashierDailyBalanceListModel> PrepareEventCashierDailyBalanceListModel(CashierDailyBalanceSearchModel searchModel)
        {
            var eventCashierDailyBalances = await _cashierEventService.GetEventCashiersDailyBalancesAsync(searchModel.EventId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            return new CashierDailyBalanceListModel().PrepareToGrid(searchModel, eventCashierDailyBalances, () =>
            {
                return eventCashierDailyBalances.Select(x => new CashierDailyBalanceItemModel
                {
                    Id = x.CashierDailyBalance.Id,
                    CashierEmail = x.Customer.Email,
                    OpeningFundBalanceAmount = x.CashierDailyBalance.OpeningFundBalanceAmount,
                    Day = x.CashierDailyBalance.Day
                });
            });
        }

        public async Task<CustomerListModel> PrepareCashierListModelAsync(int eventId, CustomerSearchModel searchModel)
        {
            var cashierRole = await _customerService.GetCustomerRoleBySystemNameAsync(DefaultValues.CashierRoleName);
            var cashiers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { cashierRole.Id }, pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);


            var eventCashiers = await _cashierEventService.GetEventCashiersAsync(eventId);

            var notAssociatedCashiers = cashiers.Where(x => eventCashiers.Select(ce => ce.CustomerId).Contains(x.Id) == false).ToArray()
                .ToPagedList(searchModel);


            return await new CustomerListModel().PrepareToGridAsync(searchModel, notAssociatedCashiers, () =>
            {
                return notAssociatedCashiers.SelectAwait(async cashier => new CustomerModel()
                {
                    Id = cashier.Id,
                    FullName = await _customerService.GetCustomerFullNameAsync(cashier),
                    Email = cashier.Email,
                });
            });
        }

        public async Task<CashierEventsListModel> PrepareEventCashierEventsListModelAsync(EventCashierEventsViewModel viewModel)
        {
            var pageIndex = viewModel.Page - 1;
            var cashierEvents = await _cashierEventService.GetAllEventCashierEventsAsync(viewModel.EventId, pageIndex, viewModel.PageSize);

            return await new CashierEventsListModel().PrepareToGridAsync(viewModel, cashierEvents, () =>
            {
                return cashierEvents.SelectAwait(async cashierEvent =>
                {
                    var customer = await _customerService.GetCustomerByIdAsync(cashierEvent.CustomerId);
                    var avatarPictureId =
                        await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute);
                    return new CashierEventsItemModel
                    {
                        Id = cashierEvent.Id,
                        EventId = viewModel.EventId,
                        CustomerId = customer.Id,
                        CashierEmail = customer.Email,
                        IsRefundPermitted = cashierEvent.IsRefundPermitted,
                        CommissionAmount = cashierEvent.CommissionAmount,
                        OpeningFundBalanceAmount = await _cashierBalanceService.GetBalanceAsync(cashierEvent.Id),
                        CashierPictureUrl = await _pictureService
                            .GetPictureUrlAsync(avatarPictureId, _mediaSettings.CartThumbPictureSize, _customerSettings.DefaultAvatarEnabled,
                                defaultPictureType: PictureType.Avatar),
                        Active = cashierEvent.Active,
                    };
                });
            });
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Services.DomainServices;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Services.Customers;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    [Permission(PermissionProvider.PushNotificationManagementName)]
    [Area(AreaNames.Admin)]
    // [License(PluginName = DefaultValue.SystemName, PageType = PageType.AdminPage)]
    public class SendNotificationController : DefaultDomainController<PushNotificationItem, CustomerNotificationModel>
    {
        private readonly ICustomerService _customerService;
        private readonly IPushNotificationTokenService _pushNotificationTokenService;

        public SendNotificationController(ICustomerService customerService,
            IPushNotificationTokenService pushNotificationTokenService,
            IDomainService<PushNotificationItem, CustomerNotificationModel> service = null,
            IDomainFactory<PushNotificationItem, CustomerNotificationModel> factory = null) : base(service, factory)
        {
            _customerService = customerService;
            _pushNotificationTokenService = pushNotificationTokenService;
        }

        [HttpGet]
        public async Task<IActionResult> SendNotification()
        {
            SetActiveMenu("SendNotification");
            var treeItems = await GetTreeItemsAsync();
            ViewBag.treeItems = JsonConvert.SerializeObject(treeItems);
            return View("SendNotification.cshtml");
        }
        
        // [HttpPost]
        public async Task<IActionResult> ListAsync(NotificationSearchModel searchModel)
        {
            var query = _domainService.GetQueryable()
                .Where(x=>x.NotificationPlatform==searchModel.NotificationPlatform)
                .Where(x=>x.NotificationScopeType==searchModel.NotificationScopeType)
                .Where(x=>x.OnDateTime>searchModel.From && x.OnDateTime<=searchModel.To);
            return await PrepareGridResponseAsync(searchModel, query);
        }

        public override Task<IActionResult> AddOrEditAsync(CustomerNotificationModel model)
        {
            return base.AddOrEditAsync(model);
        }

        // [HttpGet]
        public override async Task<IActionResult> ListAsync() => await BaseDefaultListAsync(await GetListViewModelAsync());

        protected override async Task<List<CustomerNotificationModel>> GetModelsAsync(
            IEnumerable<PushNotificationItem> entities)
        {
            var models = await _domainFactory.GetModelsAsync(entities);
            return models;
        }

        private async Task<IEnumerable<TreeItem>> GetTreeItemsAsync()
        {
            var tokens = await _pushNotificationTokenService.GetPushNotificationTokensAsync(null);
            var customerIds = tokens.Select(item => item.CustomerId).Distinct().ToArray();

            var allCustomers = await _customerService.GetCustomersByIdsAsync(customerIds);
            var treeItems = allCustomers.Select(item => new TreeItem
            {
                id = item.Id.ToString(),
                title = $"{item.Id}-{item.Email}"
            });
            return treeItems;
        }

        private static DefaultListViewModel _defaultListViewModel;

        private async Task<DefaultListViewModel> GetListViewModelAsync()
        {
            return _defaultListViewModel ??=
                new(controllerName: "SendNotification", systemName: DefaultValue.SystemName,typeof(CustomerNotificationModel))
                {
                    PageTitle = await GetResAsync("NotificationList"),
                    DeleteAction = null,
                    UpdateAction = null,
                    AddAction = null,
                    
                };
        }

        protected override string SystemName => DefaultValue.SystemName;
        protected override string FolderName => "SendNotifications";
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
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
    public class PushNotificationRelCustomerController :
        BaseDomainController<PushNotificationItemRelCustomer, PushNotificationItemRelCustomerModel>
    {
        private readonly ICustomerService _customerService;

        public PushNotificationRelCustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("/Admin/PushNotificationRelCustomer/List/{id}")]
        public async Task<IActionResult> ListAsync([FromRoute] int id)
        {
            SetActiveMenu(SystemName, "SendNotification", "List");
            
            var defaultListViewModel = new DefaultListViewModel(controllerName: "PushNotificationRelCustomer",
                systemName: DefaultValue.SystemName, typeof(PushNotificationItemRelCustomerModel))
            {
                PageTitle = "Customers",
                DeleteAction = null,
                UpdateAction = null,
                AddAction = null,
                FilterItems = new List<FilterItem>
                {
                    new("NotificationId", id)
                    {
                        IsHiddenField = true
                    }
                }
            };

            return await BaseDefaultListAsync(defaultListViewModel);
        }

        // [HttpPost]
        public async Task<IActionResult> ListAsync(NotificationSearchModel searchModel)
        {
            var query = _domainService.GetQueryable()
                .Where(i => i.PushNotificationItemId == searchModel.NotificationId);
            return await PrepareGridResponseAsync(searchModel, query);
        }

        protected override async Task<List<PushNotificationItemRelCustomerModel>> GetModelsAsync(
            IEnumerable<PushNotificationItemRelCustomer> entities)
        {
            var models = await _domainFactory.GetModelsAsync(entities);
            var customerIds = models.Select(i => i.CustomerId).ToArray();
            var customers = await _customerService.GetCustomersByIdsAsync(customerIds);

            models = models.Join(customers, n => n.CustomerId, c => c.Id, (n, c) =>
            {
                n.CustomerName = c.Email;
                return n;
            }).ToList();

            return models;
        }

        protected override string SystemName => DefaultValue.SystemName;
    }

    public class NotificationSearchModel : ExtendedSearchModel
    {
        public int NotificationId { get; set; }
        public NotificationPlatform? NotificationPlatform { get; set; }
        public NotificationScopeType NotificationScopeType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
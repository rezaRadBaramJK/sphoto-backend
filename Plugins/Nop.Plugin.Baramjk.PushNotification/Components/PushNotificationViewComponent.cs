using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents;
using Nop.Services.Customers;

namespace Nop.Plugin.Baramjk.PushNotification.Components
{
    [ViewComponent(Name = "PushNotificationComponent")]
    public class PushNotificationViewComponent : BaramjkViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        public PushNotificationViewComponent(ICustomerService customerService, IWorkContext workContext)
        {
            _customerService = customerService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isRegistered = await _customerService.IsRegisteredAsync(customer);

            return View("PushNotificationComponent.cshtml", isRegistered);
        }

        protected override string SystemName => DefaultValue.SystemName;
    }
}
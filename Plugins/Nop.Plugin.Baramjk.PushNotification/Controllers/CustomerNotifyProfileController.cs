using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Factories.Api;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Services;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    public class CustomerNotifyProfileController : BaseBaramjkApiController
    {
        private readonly IWorkContext _workContext;
        private readonly CustomerNotifyProfileService _customerNotifyProfileService;
        private readonly CustomerNotifyProfileFactory _customerNotifyProfileFactory;

        public CustomerNotifyProfileController(
            IWorkContext workContext,
            CustomerNotifyProfileService customerNotifyProfileService,
            CustomerNotifyProfileFactory customerNotifyProfileFactory)
        {
            _workContext = workContext;
            _customerNotifyProfileService = customerNotifyProfileService;
            _customerNotifyProfileFactory = customerNotifyProfileFactory;
        }

        [HttpGet("/FrontendApi/PushNotification/Customer/Profile")]
        public async Task<IActionResult> GetAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerNotifyProfile = await _customerNotifyProfileService.GetByCustomerIdAsync(customer.Id);

            var dto = _customerNotifyProfileFactory.PrepareDto(customerNotifyProfile);
            return ApiResponseFactory.Success(dto);
        }

        [HttpPost("/FrontendApi/PushNotification/Customer/Profile")]
        public async Task<IActionResult> SubmitAsync([FromBody] CustomerNotifyProfileDto apiParams)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var customerNotifyProfile = await _customerNotifyProfileService.GetByCustomerIdAsync(customer.Id);
            if (customerNotifyProfile == null)
            {
                customerNotifyProfile = new CustomerNotifyProfileEntity
                {
                    CustomerId = customer.Id,
                    Sale = apiParams.Sale,
                    Discount = apiParams.Discount,
                    BackInStock = apiParams.BackInStock
                };

                await _customerNotifyProfileService.InsertAsync(customerNotifyProfile);
            }
            else
            {
                customerNotifyProfile.Sale = apiParams.Sale;
                customerNotifyProfile.Discount = apiParams.Discount;
                customerNotifyProfile.BackInStock = apiParams.BackInStock;

                await _customerNotifyProfileService.UpdateAsync(customerNotifyProfile);
            }
            
            return ApiResponseFactory.Success();
        }
    }
}
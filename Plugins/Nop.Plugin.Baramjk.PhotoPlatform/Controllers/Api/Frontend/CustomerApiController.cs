using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Customer;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class CustomerApiController : BaseBaramjkApiController
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;

        public CustomerApiController(IWorkContext workContext,
            ICustomerService customerService, IAddressService addressService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _addressService = addressService;
        }

        [HttpPost("/FrontendApi/PhotoPlatform/Customer/SubmitGuestInfo")]
        public async Task<IActionResult> SubmitGuestInfoAsync(SubmitGuestInfoApiParams apiParams)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();


            if (await _customerService.IsGuestAsync(customer) == false || customer.IsSearchEngineAccount())
            {
                return ApiResponseFactory.BadRequest("You should be a guest");
            }

            if (string.IsNullOrEmpty(apiParams.Email))
            {
                return ApiResponseFactory.BadRequest("Email is required");
            }

            var address = new Address()
            {
                Email = apiParams.Email,
                PhoneNumber = apiParams.PhoneNumber,
                FirstName = apiParams.FullName,
            };

            await _addressService.InsertAddressAsync(address);
            await _customerService.InsertCustomerAddressAsync(customer, address);


            return ApiResponseFactory.Success();
        }
    }
}
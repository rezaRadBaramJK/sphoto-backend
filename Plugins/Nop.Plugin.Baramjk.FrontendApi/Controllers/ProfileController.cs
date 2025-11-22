using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Profiles;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class ProfileController : BaseNopWebApiFrontendController
    {
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IProfileModelFactory _profileModelFactory;

        public ProfileController(CustomerSettings customerSettings,
            ICustomerService customerService,
            IProfileModelFactory profileModelFactory)
        {
            _customerSettings = customerSettings;
            _customerService = customerService;
            _profileModelFactory = profileModelFactory;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProfileIndexModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Index([FromQuery] int? id, [FromQuery] int? pageNumber)
        {
            if (!_customerSettings.AllowViewingProfiles)
                return ApiResponseFactory.BadRequest();

            var customerId = 0;
            if (id.HasValue) customerId = id.Value;

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null || await _customerService.IsGuestAsync(customer))
                return ApiResponseFactory.NotFound($"Customer by id={customerId} not found or not registered.");

            var model = await _profileModelFactory.PrepareProfileIndexModelAsync(customer, pageNumber);

            return ApiResponseFactory.Success(model.ToDto<ProfileIndexModelDto>());
        }
    }
}
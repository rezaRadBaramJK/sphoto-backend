using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.PhotoPlatform.Factories;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.ShoppingCart;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Controllers.Api.Frontend
{
    public class ShoppingCartApiController : BaseBaramjkApiController
    {
        private readonly PhotoPlatformShoppingCartService _photoPlatformShoppingCartService;
        private readonly PhotoPlatformShoppingCartFactory _photoPlatformShoppingCartFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly EventDetailsService _eventDetailsService;

        public ShoppingCartApiController(PhotoPlatformShoppingCartService photoPlatformShoppingCartService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            PhotoPlatformShoppingCartFactory photoPlatformShoppingCartFactory,
            IGenericAttributeService genericAttributeService,
            IAddressService addressService,
            ICustomerService customerService,
            EventDetailsService eventDetailsService)
        {
            _photoPlatformShoppingCartService = photoPlatformShoppingCartService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _photoPlatformShoppingCartFactory = photoPlatformShoppingCartFactory;
            _genericAttributeService = genericAttributeService;
            _addressService = addressService;
            _customerService = customerService;
            _eventDetailsService = eventDetailsService;
        }

        private async Task AddBillingAddressForCustomerBasedOnEventCountryAsync(Customer customer, int eventId)


        {
            //we need to know the country for additional payment method fees,
            //in normal scenario the customer sets a billing address.
            //however here we set a billing address for the customer based on the country in which the event is being held 

            var eventDetails = await _eventDetailsService.GetByEventIdAsync(eventId);

            var customerFirstName =
                await _genericAttributeService.GetAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, defaultValue: string.Empty);
            var customerLastName =
                await _genericAttributeService.GetAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, defaultValue: string.Empty);
            var customerPhoneNumber =
                await _genericAttributeService.GetAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, defaultValue: string.Empty);


            var prevBillingAddresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);


            //! this is a temporary logic 
            var eventCountryId = (await _eventDetailsService.GetEventCountriesAsync(eventDetails.EventId)).FirstOrDefault()?.Id;

            if (eventCountryId == null)
                return;
            //!
            if (prevBillingAddresses.Any(a => a.CountryId == eventCountryId))
            {
                customer.BillingAddressId = prevBillingAddresses.First(a => a.CountryId == eventCountryId).Id;
                await _customerService.InsertCustomerAddressAsync(customer, prevBillingAddresses.First(a => a.CountryId == eventCountryId));
            }
            else
            {
                var billingAddress = new Address
                {
                    Email = customer.Email,
                    FirstName = customerFirstName,
                    LastName = customerLastName,
                    PhoneNumber = customerPhoneNumber,
                    CreatedOnUtc = DateTime.UtcNow,
                    CountryId = eventCountryId
                };


                await _addressService.InsertAddressAsync(billingAddress);
                await _customerService.InsertCustomerAddressAsync(customer, billingAddress);

                customer.BillingAddressId = billingAddress.Id;
            }

            await _customerService.UpdateCustomerAsync(customer);
        }

        [HttpGet("/FrontendApi/PhotoPlatform/ShoppingCart")]
        public async Task<IActionResult> GetShoppingCartAsync()
        {
            var result = await _photoPlatformShoppingCartFactory.PrepareShoppingCartAsync();
            return ApiResponseFactory.Success(result);
        }

        [HttpPost("/FrontendApi/PhotoPlatform/ShoppingCart")]
        public async Task<IActionResult> AddToCartAsync([FromBody] SubmitShoppingCartItemsApiModel model)
        {
            if (model.Items.Select(i => i.EventId).Distinct().Count() > 1)
                return ApiResponseFactory.BadRequest("You cannot add more than one event");

            var result = await _photoPlatformShoppingCartService.ProcessAddingItemToCartAsync(model, true);

            if (result.IsSuccess)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                await AddBillingAddressForCustomerBasedOnEventCountryAsync(customer, model.Items.Select(i => i.EventId).First());
                return ApiResponseFactory.Success();
            }

            return ApiResponseFactory.BadRequest(result.Message);
        }

        [HttpPut("/FrontendApi/PhotoPlatform/ShoppingCart")]
        public async Task<IActionResult> UpdateCartAsync([FromBody] SubmitShoppingCartItemsApiModel model)
        {
            if (model.Items.Select(i => i.EventId).Distinct().Count() > 1)
                return ApiResponseFactory.BadRequest("You cannot add more than one event");

            var result = await _photoPlatformShoppingCartService.ProcessUpdatingShoppingCartAsync(model, true);

            if (result.IsSuccess)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                await AddBillingAddressForCustomerBasedOnEventCountryAsync(customer, model.Items.Select(i => i.EventId).First());
                return ApiResponseFactory.Success();
            }

            return ApiResponseFactory.BadRequest(result.Message);
        }

        [HttpDelete("/FrontendApi/PhotoPlatform/ShoppingCart")]
        public async Task<IActionResult> ClearAsync(
            [FromQuery] ShoppingCartType? shoppingCartType = ShoppingCartType.ShoppingCart)
        {
            var cartItems = await _shoppingCartService
                .GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), shoppingCartType);

            //todo: use one query to delete them. be careful in bulk delete, The delete entity event will not be raised by nop commerce
            foreach (var cartItem in cartItems)
                await _shoppingCartService.DeleteShoppingCartItemAsync(cartItem);


            return ApiResponseFactory.Success();
        }


        [HttpDelete("/FrontendApi/PhotoPlatform/ShoppingCart/DeleteEvent/{id:int}")]
        public async Task<IActionResult> DeleteEventCartItemAsync(int id)
        {
            var result = await _photoPlatformShoppingCartService.DeleteEventItemFromCartAsync(id);
            return result.IsSuccess ? ApiResponseFactory.Success() : ApiResponseFactory.BadRequest(result.Message);
        }


        [HttpDelete("/FrontendApi/PhotoPlatform/ShoppingCart/Items")]
        public async Task<IActionResult> DeleteCartItemsAsync(DeleteItemsFromCartApiModel apiModel)
        {
            if (apiModel.Items.Any() == false)
            {
                return ApiResponseFactory.BadRequest("No items were found to delete");
            }

            var result = await _photoPlatformShoppingCartService.DeleteShoppingCartItemAsync(apiModel);
            return result.IsSuccess ? ApiResponseFactory.Success() : ApiResponseFactory.BadRequest(result.Message);
        }

        [HttpGet("/FrontendApi/PhotoPlatform/ShoppingCart/Validate")]
        public async Task<IActionResult> ValidateCartAsync()
        {
            var result = await _photoPlatformShoppingCartService.ValidateShoppingCartAsync();
            return result.IsSuccess ? ApiResponseFactory.Success(result.IsSuccess) : ApiResponseFactory.BadRequest(result.Message);
        }
    }
}
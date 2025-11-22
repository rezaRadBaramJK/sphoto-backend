using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Services;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Services.Payments;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs
{
  public class PhotoPlatformContactUsPaymentService
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly ITranslationService _translationService;
        private readonly IGatewayClientProvider _gatewayClientProvider;
        private readonly IGatewayService _gatewayService;
        
        public PhotoPlatformContactUsPaymentService(
            IWorkContext workContext,
            IStoreContext storeContext,
            IPaymentPluginManager paymentPluginManager,
            ITranslationService translationService, 
            IGatewayClientProvider gatewayClientProvider,
            IGatewayService gatewayService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _paymentPluginManager = paymentPluginManager;
            _translationService = translationService;
            _gatewayClientProvider = gatewayClientProvider;
            _gatewayService = gatewayService;
        }

        public async Task<IList<IPaymentMethod>> GetPaymentMethodsAsync(
            int countryId,
            Customer customer = null,
            Store store = null
            )
        {
            customer ??= await _workContext.GetCurrentCustomerAsync();
            store ??= await _storeContext.GetCurrentStoreAsync();

            var paymentMethods = 
                await _paymentPluginManager
                .LoadActivePluginsAsyncAsync(customer, store.Id, countryId);
            
            return paymentMethods
                .Where(pm => pm.PaymentMethodType is PaymentMethodType.Standard or PaymentMethodType.Redirection)
                .ToList();
        }


        public async Task<string> GetPaymentUrlAsync(GetPaymentUrlServiceParams serviceParams)
        {
            if (serviceParams == null)
                throw new ArgumentNullException(nameof(serviceParams));
            
            var customer = await _workContext.GetCurrentCustomerAsync();
            
            var createTranslationRequest = new CreateTranslationRequest
            {
                AmountToPay = serviceParams.Price,
                CustomerId = customer.Id,
                ConsumerName = DefaultValues.ConsumerName,
                ConsumerEntityType = DefaultValues.TableNamePrefix,
                ConsumerEntityId = serviceParams.ContactInfoId,
                ConsumerCallBack = "/FrontendApi/PhotoPlatform/ContactUs/Payment/Verify?guid={0}",
                ConsumerData = ""
            };
            
            var translation = await _translationService.NewTranslationAsync(createTranslationRequest);
            var paymentRequest = new InvoiceRequest
            {
                FirstName = serviceParams.CustomerFirstName,
                LastName = serviceParams.CustomerLastName,
                PhoneNumber = serviceParams.CustomerPhoneNumber,
                Email = serviceParams.CustomerEmail,
                Amount = translation.AmountToPay,
            };
            
            var gatewayClient = _gatewayClientProvider.GetDefaultGatewayClient();
            var response = await _gatewayService.SendInvoiceAsync(gatewayClient, translation, paymentRequest);

            return response.Body.PaymentUrl;

        }
        
    }
}
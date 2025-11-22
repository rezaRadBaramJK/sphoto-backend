using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models.Customers;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Services.Customers;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments
{
    public abstract class BaseConsumerPaymentService
    {
        private readonly ICustomerService _customerService;
        private readonly IGatewayService _gatewayService;
        private readonly ITranslationService _translationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerDtoFactory _customerDtoFactory;
        private readonly IGatewayClientProvider _gatewayClientProvider;
        private readonly ITranslationVerifyService _translationVerifyService;

        protected BaseConsumerPaymentService()
        {
            _customerService = EngineContext.Current.Resolve<ICustomerService>();
            _gatewayService = EngineContext.Current.Resolve<IGatewayService>();
            _translationService = EngineContext.Current.Resolve<ITranslationService>();
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _customerDtoFactory = EngineContext.Current.Resolve<ICustomerDtoFactory>();
            _gatewayClientProvider = EngineContext.Current.Resolve<IGatewayClientProvider>();
            _translationVerifyService = EngineContext.Current.Resolve<ITranslationVerifyService>();
        }

        protected abstract string ConsumerName { get; }
        protected abstract string ConsumerEntityType { get; }
        protected virtual string ConsumerCallBack => $"/{ConsumerName}/CallBack?guid={0}";

        protected virtual async Task<string> SendInvoiceAsync(int entityId, decimal amountToPay)
        {
            var customer = await GetCustomerAsync();
            var translation = await CreateGatewayPaymentTranslationAsync(entityId, amountToPay, customer.Id);
            var customerDto = await GetCustomerInfo(customer);
            var paymentRequest = CreatePaymentRequest(entityId, amountToPay, customerDto);
            var gatewayClient = GetGatewayClient();
            await _gatewayService.SendInvoiceAsync(gatewayClient, translation, paymentRequest);
            return translation.PaymentUrl;
        }

        protected virtual async Task<GatewayPaymentTranslationResponse> CreateTranslationAsync(int entityId,
            decimal amountToPay)
        {
            var customer = await GetCustomerAsync();
            var translation = await CreateGatewayPaymentTranslationAsync(entityId, amountToPay, customer.Id);
            var responseModel = translation.ToResponseModel();
            return responseModel;
        }

        public async Task<bool> HandleTranslationStatusEventAsync(GatewayPaymentTranslationStatusEvent statusEvent)
        {
            if (statusEvent.JustPaid == false)
                return false;

            var translation = statusEvent.Entity;
            if (translation.ConsumerName != ConsumerName)
                return false;

            if (translation.NeedToProcess() == false)
                return false;

            var consumerResult = await ProcessTranslationStatusEventAsync(statusEvent);
            if (consumerResult == null)
                return false;

            statusEvent.Handle(consumerResult);

            await _translationService.SetConsumerStatusAsync(translation, consumerResult.ConsumerStatus);
            return consumerResult.ConsumerStatus == ConsumerStatus.Success;
        }

        public abstract Task<ConsumerResult> ProcessTranslationStatusEventAsync(
            GatewayPaymentTranslationStatusEvent statusEvent);

        public async Task<VerifyResult> SelfVerifyAsync(string invoiceId,
            Func<VerifyResult, IGatewayClient, ConsumerStatus?> callBack)
        {
            var gatewayClient = GetGatewayClient();
            var verifyResult = await _translationVerifyService.VerifyAsync(gatewayClient,
                new GetInvoiceStatusRequest(invoiceId));

            if (verifyResult.IsNewSuccessPaid == false)
                return verifyResult;

            if (verifyResult.Translation.ConsumerStatus == ConsumerStatus.Success)
                return verifyResult;

            var translation = verifyResult.Translation;

            var result = callBack(verifyResult, gatewayClient);
            if (result != null)
                await _translationService.SetConsumerStatusAsync(translation.Guid, result.Value);

            return verifyResult;
        }

        protected virtual IGatewayClient GetGatewayClient(string name = null)
        {
            return name.HasValue()
                ? _gatewayClientProvider.GetGatewayClient(name)
                : _gatewayClientProvider.GetDefaultGatewayClient();
        }

        protected virtual InvoiceRequest CreatePaymentRequest(int entityId, decimal amountToPay, CustomerDto info)
        {
            return new InvoiceRequest
            {
                FirstName = info.FirstName,
                LastName = info.LastName,
                PhoneNumber = info.Phone,
                Email = info.Email,
                Amount = amountToPay,
                ClientReferenceId = entityId.ToString()
            };
        }

        protected virtual async Task<Customer> GetCustomerAsync() => await _workContext.GetCurrentCustomerAsync();

        protected virtual async Task<CustomerDto> GetCustomerInfo(Customer customer)
        {
            var address = await _customerService.GetCustomerBillingAddressAsync(customer);
            if (address == null)
                return await _customerDtoFactory.PrepareCustomerDtoAsync(customer);
            return await _customerDtoFactory.PrepareCustomerDtoAsync(customer, address.Id);
        }

        protected virtual async Task<GatewayPaymentTranslation> CreateGatewayPaymentTranslationAsync(int entityId,
            decimal amountToPay, int customerId)
        {
            return await _translationService.NewTranslationAsync(new CreateTranslationRequest
            {
                AmountToPay = amountToPay,
                CustomerId = customerId,
                ConsumerName = ConsumerName,
                ConsumerEntityType = ConsumerEntityType,
                ConsumerEntityId = entityId,
                ConsumerCallBack = ConsumerCallBack,
                ConsumerData = ""
            });
        }
    }
}
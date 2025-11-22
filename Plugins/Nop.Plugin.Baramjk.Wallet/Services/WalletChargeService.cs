using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Services.Currencies;
using Nop.Plugin.Baramjk.Framework.Services.Networks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Domain;
using Nop.Plugin.Baramjk.Wallet.Services.Models.WalletChargeServices;

namespace Nop.Plugin.Baramjk.Wallet.Services
{
    public class WalletChargeService
    {
        private readonly IRepository<WalletItemPackage> _walletItemPackageRepository;
        private readonly IWalletService _walletService;
        private readonly ITranslationService _translationService;
        private readonly ICurrencyTools _currencyTools;
        private readonly IWorkContext _workContext;
        private readonly IGatewayClientProvider _gatewayClientProvider;
        private readonly IGatewayService _gatewayService;
        private readonly ICustomerDtoFactory _customerDtoFactory;

        public WalletChargeService(IRepository<WalletItemPackage> walletItemPackageRepository,
            IWalletService walletService, ITranslationService translationService, ICurrencyTools currencyTools,
            IWorkContext workContext, IGatewayClientProvider gatewayClientProvider, IGatewayService gatewayService,
            ICustomerDtoFactory customerDtoFactory)
        {
            _walletItemPackageRepository = walletItemPackageRepository;
            _walletService = walletService;
            _translationService = translationService;
            _currencyTools = currencyTools;
            _workContext = workContext;
            _gatewayClientProvider = gatewayClientProvider;
            _gatewayService = gatewayService;
            _customerDtoFactory = customerDtoFactory;
        }

        public async Task<HttpResponse<CreateInvoiceResponse>> ChargePaymentAsync(decimal amount, string currencyCode)
        {
            var walletChargeModels = new List<WalletChargeModel>
            {
                new()
                {
                    Amount = amount,
                    CurrencyCode = currencyCode
                }
            };
            var amountToPay = await CalculatePriceAsync(walletChargeModels);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var createTranslationRequest = new CreateTranslationRequest
            {
                AmountToPay = amountToPay,
                CustomerId = customer.Id,
                ConsumerName = DefaultValue.ConsumerName,
                ConsumerEntityType = "WalletChargeModel",
                ConsumerEntityId = 0,
                ConsumerCallBack = "/WalletCharge/ChargePaymentCallBack?guid={0}",
                ConsumerData = walletChargeModels.ToJson()
            };
            var translation = await _translationService.NewTranslationAsync(createTranslationRequest);
            var response = await SendInvoiceAsync(translation);
            return response;
        }

        public async Task<HttpResponse<CreateInvoiceResponse>> ChargePaymentByPackageIdAsync(int packageId)
        {
            var translation = await CreateGatewayPaymentTranslationByPackageId(packageId);
            var response = await SendInvoiceAsync(translation);
            return response;
        }

        public async Task<GatewayPaymentTranslation> CreateGatewayPaymentTranslationByPackageId(int packageId)
        {
            var walletChargeModels = await GetWalletChargeModelsAsync(packageId);
            var amountToPay = await CalculatePriceAsync(walletChargeModels);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var createTranslationRequest = new CreateTranslationRequest
            {
                AmountToPay = amountToPay,
                CustomerId = customer.Id,
                ConsumerName = DefaultValue.ConsumerName,
                ConsumerEntityType = "Package",
                ConsumerEntityId = packageId,
                ConsumerCallBack = "/WalletCharge/ChargePaymentCallBack?guid={0}",
                ConsumerData = ""
            };
            var translation = await _translationService.NewTranslationAsync(createTranslationRequest);
            return translation;
        }

        public async Task<HttpResponse<CreateInvoiceResponse>> ChargePaymentByAmountAsync(decimal amount,
            string currencyCode)
        {
            var translation = await CreateGatewayPaymentTranslationByAmountAsync(amount, currencyCode);
            var response = await SendInvoiceAsync(translation);
            return response;
        }

        public async Task<GatewayPaymentTranslation> CreateGatewayPaymentTranslationByAmountAsync(decimal amount,
            string currencyCode)
        {
            var walletChargeModels = new List<WalletChargeModel>
            {
                new()
                {
                    Amount = amount,
                    CurrencyCode = currencyCode
                }
            };
            var amountToPay = await CalculatePriceAsync(walletChargeModels);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var createTranslationRequest = new CreateTranslationRequest
            {
                AmountToPay = amountToPay,
                CustomerId = customer.Id,
                ConsumerName = DefaultValue.ConsumerName,
                ConsumerEntityType = "WalletChargeModel",
                ConsumerEntityId = 0,
                ConsumerCallBack = "/WalletCharge/ChargePaymentCallBack?guid={0}",
                ConsumerData = walletChargeModels.ToJson()
            };
            var translation = await _translationService.NewTranslationAsync(createTranslationRequest);
            return translation;
        }

        public async Task<bool> HandleTranslationStatusEventAsync(GatewayPaymentTranslationStatusEvent statusEvent)
        {
            var translation = statusEvent.Entity;
            if (translation.NeedToProcess() == false)
                return false;

            var customerId = translation.OwnerCustomerId;
            if (translation.ConsumerEntityType == "WalletChargeModel")
            {
                var walletChargeModels = translation.ConsumerData.DeserializeFromJson<List<WalletChargeModel>>();
                await ChargeAsync(customerId, walletChargeModels);
            }
            else if (translation.ConsumerEntityType == "Package")
            {
                var chargeModels = await GetWalletChargeModelsAsync(translation.ConsumerEntityId);
                await ChargeAsync(customerId, chargeModels);
            }

            await _translationService.SetConsumerStatusAsync(translation, ConsumerStatus.Success);
            statusEvent.Handle(new ConsumerResult
            {
                Message = "Success",
                ConsumerPayload = null,
                ConsumerStatus = ConsumerStatus.Success,
            });

            return true;
        }

        private async Task<HttpResponse<CreateInvoiceResponse>> SendInvoiceAsync
            (GatewayPaymentTranslation translation)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var info = await _customerDtoFactory.PrepareCustomerDtoAsync(customer.Id);
            var paymentRequest = new InvoiceRequest
            {
                FirstName = info.FirstName,
                LastName = info.LastName,
                PhoneNumber = info.Phone,
                Email = info.Email,
                Amount = translation.AmountToPay,
            };

            var gatewayClient = _gatewayClientProvider.GetDefaultGatewayClient();
            var response = await _gatewayService.SendInvoiceAsync(gatewayClient, translation, paymentRequest);
            return response;
        }

        private async Task<List<WalletChargeModel>> GetWalletChargeModelsAsync(int packageId)
        {
            var chargeModels = await _walletItemPackageRepository.Table
                .Where(item => item.WalletPackageId == packageId)
                .Select(item => new WalletChargeModel
                {
                    Amount = item.Amount,
                    CurrencyCode = item.CurrencyCode.Trim()
                }).ToListAsync();

            return chargeModels;
        }

        private async Task ChargeAsync(int customerId, List<WalletChargeModel> chargeModels)
        {
            foreach (var item in chargeModels)
            {
                await _walletService.PerformAsync(new WalletTransactionRequest
                {
                    CustomerId = customerId,
                    CurrencyCode = item.CurrencyCode,
                    Amount = item.Amount,
                    Type = WalletHistoryType.Charge,
                    Note = "wallet charge from service"
                });
            }
            
        }

        private async Task<decimal> CalculatePriceAsync(List<WalletChargeModel> walletChargeModels)
        {
            var toPay = await walletChargeModels.SumAwaitAsync(async item =>
                (await _currencyTools.ConvertPrimaryToAsync(item.Amount, item.CurrencyCode)).Amount);

            return toPay;
        }
    }
}
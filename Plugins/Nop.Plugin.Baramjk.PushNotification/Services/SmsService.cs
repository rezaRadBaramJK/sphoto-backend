using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.Framework.Services.Sms;
using Nop.Plugin.Baramjk.PushNotification.Exceptions;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Services.Providers;
using Nop.Services.Customers;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public interface ISmsServiceFactory
    {
        ISmsProvider GetService();
    }

    public class SmsServiceFactory : ISmsServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SmsProviderSetting _smsProviderSetting;


        public SmsServiceFactory(IServiceProvider serviceProvider,
            SmsProviderSetting smsProviderSetting)
        {
            _serviceProvider = serviceProvider;
            _smsProviderSetting = smsProviderSetting;
        }

        public ISmsProvider GetService()
        {
            if (Enum.TryParse<SmsProviderEnum>(_smsProviderSetting.ProviderName, out var providerEnum) == false)
            {
                throw new ArgumentException($"Invalid SMS provider name: {_smsProviderSetting.ProviderName}");
            }


            return providerEnum switch
            {
                SmsProviderEnum.RmlConnect => _serviceProvider.GetRequiredService<RmlConnectSmsProvider>(),
                SmsProviderEnum.FutureCommunicationsCompany => _serviceProvider.GetRequiredService<FCCSmsProvider>(),
                _ => throw new ArgumentOutOfRangeException(nameof(providerEnum), "Unknown SMS provider")
            };
        }
    }

    public class SmsService : ISmsService
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;
        private  ISmsProvider _smsProvider;
        private readonly IMobileService _mobileService;
        private readonly ISmsServiceFactory _smsServiceFactory;


        private readonly IRepository<SendSmsModel> _repository;

        public SmsService(ICustomerService customerService, ILogger logger,
            IMobileService mobileService, IRepository<SendSmsModel> repository,
            ISmsServiceFactory smsServiceFactory, ISmsProvider smsProvider)
        {
            _customerService = customerService;
            _logger = logger;
            _mobileService = mobileService;
            _repository = repository;
            _smsServiceFactory = smsServiceFactory;
            _smsProvider = smsProvider;
        }

        private ISmsProvider SmsProvider => _smsProvider ??= _smsServiceFactory.GetService();

        public async Task SendSms(int customerId, string message)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (!_mobileService.IsValidMobileNumber(customer.Username))
            {
                await _logger.ErrorAsync($"invalid mobile number for user={customerId} phone={message}");
            }

            await SendSms(customer.Username, message, customerId);
        }

        public async Task SendSms(string receptor, string message, int customerId = default)
        {
            if (!_mobileService.IsValidMobileNumber(receptor))
            {
                await _logger.ErrorAsync($"invalid mobile number for user={customerId} phone={message}");
            }

            var smsModel = new SendSmsModel
            {
                CustomerId = customerId,
                DateTime = DateTime.Now,
                // = DateTime.Now,
                Receptor = receptor,
                Status = SendSmsStatus.Pending,
                Text = message
            };

            await _repository.InsertAsync(smsModel);
            try
            {
                await SmsProvider.SendMessageAsync(receptor, message);
                smsModel.Status = SendSmsStatus.Sent;
                await _repository.UpdateAsync(smsModel);
            }
            catch (Exception e)
            {
                smsModel.Status = SendSmsStatus.Failed;
                await _repository.UpdateAsync(smsModel);

                throw new SendSmsException();
            }
        }
    }
}
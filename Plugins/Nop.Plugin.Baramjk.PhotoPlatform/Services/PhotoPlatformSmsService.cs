using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Services.Sms;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class PhotoPlatformSmsService
    {
        private readonly ISmsService _smsService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;

        public PhotoPlatformSmsService(ISmsService smsService,
            ISettingService settingService, ILogger logger,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService)
        {
            _smsService = smsService;
            _settingService = settingService;
            _logger = logger;   
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
        }

        public async Task SendTicketSmsAsync(Order order)
        {
            string customerPhoneNumber;
            var placedByCashier = await _genericAttributeService.GetAttributeAsync<int>(order, DefaultValues.OrderPlacedByCashierAttributeKey);

            if (placedByCashier != 0)
            {
                customerPhoneNumber =
                    await _genericAttributeService.GetAttributeAsync<string>(order, DefaultValues.CustomerPhoneForCashierOrderAttributeKey);
            }
            else
            {
                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                customerPhoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);
            }

            if (string.IsNullOrEmpty(customerPhoneNumber))
                return;


            var settings = await _settingService.LoadSettingAsync<PhotoPlatformSettings>();

            if (settings == null || string.IsNullOrEmpty(settings.OrderDetailsFrontendBaseUrl))
                return;

            var url = $"{settings.OrderDetailsFrontendBaseUrl}/{order.OrderGuid}";

            var message = settings.TicketSmsMessage.ReplaceSafe("{url}", url);

            await _logger.InformationAsync(message);

            await _smsService.SendSms(customerPhoneNumber, message, order.CustomerId);
        }
    }
}
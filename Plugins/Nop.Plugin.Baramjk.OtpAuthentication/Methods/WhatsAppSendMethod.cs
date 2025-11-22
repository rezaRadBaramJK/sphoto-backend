using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification.Delegates;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification.Models;
using Nop.Plugin.Baramjk.OtpAuthentication.Methods.Abstractions;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Methods;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Methods
{
    public class WhatsAppSendMethod : ISendMethod
    {
        private readonly IWhatsAppProvider _whatsAppProvider;
        private readonly ILogger _logger;

        public WhatsAppSendMethod(
            WhatsAppProviderResolver whatsAppProviderResolver,
            ILogger logger)
        {
            _whatsAppProvider = whatsAppProviderResolver.Invoke();
            _logger = logger;
        }

        public SendMethodType Type => SendMethodType.Whatsapp;
        

        public async Task SendAsync(SendOtpParams sendParams)
        {

            if (string.IsNullOrEmpty(sendParams.PhoneNumber))
            {
                await _logger.ErrorAsync($"{nameof(WhatsAppSendMethod)}: invalid phone number.");
                return;    
            }

            if (string.IsNullOrEmpty(sendParams.OtpCode))
            {
                await _logger.ErrorAsync($"{nameof(WhatsAppSendMethod)}: invalid otp code.");
                return;
            }

            if (_whatsAppProvider == null)
            {
                await _logger.ErrorAsync($"{nameof(WhatsAppSendMethod)}: invalid what's app provider.");
                return;
            }
            
            
            var sendResult = await _whatsAppProvider.SendOtpAsync(new WhatsAppSendOtpParams
            {
                PhoneNumber = sendParams.PhoneNumber,
                OtpCode = sendParams.OtpCode,
            });

            if (sendResult.IsSuccessful)
                return;

            await _logger.ErrorAsync($"{nameof(WhatsAppSendMethod)}: Send whats app message has been failed. error: {string.Join(", ", sendResult.Errors)}");

        }
    }
}
using System;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.OtpAuthentication.Methods.Abstractions;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Methods;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Methods
{
    public class SmsSendMethod: ISendMethod
    {
        private readonly ISmsProvider _smsProvider;

        public SmsSendMethod(ISmsProvider smsProvider)
        {
            _smsProvider = smsProvider;
        }

        public SendMethodType Type => SendMethodType.Sms;
        
        public async Task SendAsync(SendOtpParams sendParams)
        {
            if (string.IsNullOrEmpty(sendParams.PhoneNumber))
                throw new ArgumentNullException(nameof(sendParams.PhoneNumber));

            if (string.IsNullOrEmpty(sendParams.Message))
                throw new ArgumentNullException(nameof(sendParams.Message));

            if (string.IsNullOrEmpty(sendParams.OtpCode))
                throw new ArgumentNullException(sendParams.OtpCode);
            
            await _smsProvider.SetSetting(SmsProviderMode.Transactional);
            await _smsProvider.SendMessageAsync(sendParams.PhoneNumber, sendParams.Message);
        }
        
    }
}
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.OtpAuthentication.Methods.Abstractions;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Methods;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Methods
{
    public class EmailOrSmsSendMethod : ISendMethod
    {
        private readonly SmsSendMethod _smsSendMethod;
        private readonly EmailSendMethod _emailSendMethod;

        public EmailOrSmsSendMethod(
            EmailSendMethod emailSendMethod, SmsSendMethod smsSendMethod)
        {
            _emailSendMethod = emailSendMethod;
            _smsSendMethod = smsSendMethod;
        }

        public SendMethodType Type => SendMethodType.EmailOrSms;

        public async Task SendAsync(SendOtpParams sendParams)
        {
            if (string.IsNullOrEmpty(sendParams.Email) && string.IsNullOrEmpty(sendParams.PhoneNumber))
                throw new NopException("Both email and phone number could not be empty.");

            if (string.IsNullOrEmpty(sendParams.PhoneNumber) == false)
                await _smsSendMethod.SendAsync(sendParams);

            else if (string.IsNullOrEmpty(sendParams.Email) == false)
                await _emailSendMethod.SendAsync(sendParams);
        }
    }
}
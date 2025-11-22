using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Baramjk.OtpAuthentication.Methods.Abstractions;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Methods;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;
using Nop.Plugin.Baramjk.OtpAuthentication.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;


namespace Nop.Plugin.Baramjk.OtpAuthentication.Methods
{
    public class EmailSendMethod: ISendMethod
    {
        private readonly IEmailSender _emailSender;
        private readonly ILocalizationService _localizationService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly OtpMessageTemplateService _otpMessageTemplateService;
        private readonly IStoreContext _storeContext;
        
        public EmailSendMethod(
            IEmailSender emailSender,
            ILocalizationService localizationService,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            OtpMessageTemplateService otpMessageTemplateService,
            IStoreContext storeContext)
        {
            _emailSender = emailSender;
            _localizationService = localizationService;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _otpMessageTemplateService = otpMessageTemplateService;
            _storeContext = storeContext;
        }

        public SendMethodType Type => SendMethodType.Email;

        public async Task SendAsync(SendOtpParams sendParams)
        {
            if (string.IsNullOrEmpty(sendParams.Email))
                throw new ArgumentNullException(nameof(sendParams.Email));

            if (string.IsNullOrEmpty(sendParams.OtpCode))
                throw new ArgumentNullException(nameof(sendParams.OtpCode));

            if (_emailAccountSettings.DefaultEmailAccountId == 0)
                throw new NopException("Default email account does not configured.");

            var emailAccount =
                await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);

            if(emailAccount == null)
                throw new NopException("Default email account not found.");

            var messageTemplate = await _otpMessageTemplateService.GetOtpMessageTemplateAsync();
            
            var store = await _storeContext.GetCurrentStoreAsync();
            var storeName = await _localizationService.GetLocalizedAsync(store, s => s.Name);
            
            var subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject);
            subject = subject.Replace("%Store.Name%", storeName);
            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body);
            body = FormatString(body, new { otp = sendParams.OtpCode });
            body = body.Replace("%Store.Name%", storeName);
            
            await _emailSender.SendEmailAsync(
                emailAccount,
                subject,
                body, 
                emailAccount.Email,
                emailAccount.DisplayName,
                sendParams.Email,
                null);
        }
        
        private string FormatString(string template, object parameters)
        {
            var result = template;
            var properties = parameters.GetType().GetProperties();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(parameters, null);
                if (propertyValue == default)
                    continue;
                var pattern = "{" + propertyName + "}";
                result = result.Replace(pattern, propertyValue.ToString());
            }

            return result;
        }
        
    }
}

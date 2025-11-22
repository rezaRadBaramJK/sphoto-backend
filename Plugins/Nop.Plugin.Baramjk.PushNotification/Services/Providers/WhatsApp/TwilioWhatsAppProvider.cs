using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Services.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace Nop.Plugin.Baramjk.PushNotification.Services.Providers.WhatsApp
{
    public class TwilioWhatsAppProvider: IWhatsAppProvider
    {
        private readonly WhatsAppSettings _whatsAppSettings;
        private readonly ILogger _logger;
        
        public TwilioWhatsAppProvider(
            WhatsAppSettings whatsAppSettings,
            ILogger logger)
        {
            _whatsAppSettings = whatsAppSettings;
            _logger = logger;
        }
        
        public string ProviderName => "Twilio";
        
        public async Task<WhatAppMessageResult> SendOtpAsync(WhatsAppSendOtpParams sendParams)
        {
            if (string.IsNullOrEmpty(sendParams.PhoneNumber))
                throw new ArgumentNullException(nameof(sendParams.PhoneNumber));
        
            if (string.IsNullOrEmpty(sendParams.OtpCode))
                throw new ArgumentNullException(sendParams.OtpCode);
            
            TwilioClient.Init(_whatsAppSettings.ApiSid, _whatsAppSettings.ApiSecret);
            
            var phoneNumber = sendParams.PhoneNumber;
        
            if (phoneNumber.StartsWith("+") == false)
                phoneNumber = $"+{phoneNumber}";

            var contentVariables = JsonConvert.SerializeObject(
                new Dictionary<string, string> { { "1", sendParams.OtpCode } }, Formatting.Indented);
            
            var message = await MessageResource.CreateAsync(
                contentSid: _whatsAppSettings.OtpTemplateName,
                from: new Twilio.Types.PhoneNumber($"whatsapp:{_whatsAppSettings.SenderPhoneNumber}"),
                to: new Twilio.Types.PhoneNumber($"whatsapp:{phoneNumber}"),
                contentVariables:contentVariables
                );
        
            return message.ErrorCode.HasValue == false 
                ? WhatAppMessageResult.GetSuccessfulResult() 
                : WhatAppMessageResult.GetFailedResult($"{message.ErrorCode.Value} - {message.ErrorMessage}");
        }
        
        public async Task<WhatAppMessageResult> SendMessageAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));
        
            if (phoneNumber.StartsWith("+") == false)
                phoneNumber = $"+{phoneNumber}";
            
            TwilioClient.Init(_whatsAppSettings.ApiSid, _whatsAppSettings.ApiSecret);
            
            var sendResult = await MessageResource.CreateAsync(
                from: new Twilio.Types.PhoneNumber($"whatsapp:{_whatsAppSettings.SenderPhoneNumber}"),
                body: message,
                to: new Twilio.Types.PhoneNumber($"whatsapp:{phoneNumber}"));
                
            return sendResult.ErrorCode.HasValue == false 
                ? WhatAppMessageResult.GetSuccessfulResult() 
                : WhatAppMessageResult.GetFailedResult($"{sendResult.ErrorCode.Value} - {sendResult.ErrorMessage}");
        }
        
        public async Task<WhatAppMessageResult> SendStatusHasChangedAsync(string phoneNumber, string eventName, string statusName, string templateName = "")
        {
            
            if (string.IsNullOrEmpty(phoneNumber))
            {
                await _logger.ErrorAsync($"{nameof(TwilioWhatsAppProvider)}: Invalid phone number.");
                return WhatAppMessageResult.GetFailedResult("Invalid phone number.");
            }
        
            if (string.IsNullOrEmpty(templateName))
            {
                await _logger.ErrorAsync($"{nameof(TwilioWhatsAppProvider)}: Invalid template name.");
                return WhatAppMessageResult.GetFailedResult("Invalid template name.");
            }
            
            if (phoneNumber.StartsWith("+") == false)
                phoneNumber = $"+{phoneNumber}";
            
            TwilioClient.Init(_whatsAppSettings.ApiSid, _whatsAppSettings.ApiSecret);
            
            string message;
        
            if (string.IsNullOrEmpty(templateName))
            {
                message = $"${eventName} - {statusName}";
            }
            else
            {
                message = templateName
                    .Replace("{" + nameof(eventName) + "}", eventName)
                    .Replace("{" + nameof(statusName) + "}", statusName);
            }
            
            var sendResult = await MessageResource.CreateAsync(
                from: new Twilio.Types.PhoneNumber($"whatsapp:{_whatsAppSettings.SenderPhoneNumber}"),
                body: message,
                to: new Twilio.Types.PhoneNumber($"whatsapp:{phoneNumber}"));
            
            return sendResult.ErrorCode.HasValue == false 
                ? WhatAppMessageResult.GetSuccessfulResult() 
                : WhatAppMessageResult.GetFailedResult($"{sendResult.ErrorCode.Value} - {sendResult.ErrorMessage}");
        }
        
        
    }
}
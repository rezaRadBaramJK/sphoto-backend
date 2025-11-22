using System;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotification
{
    public interface IWhatsAppProvider
    {
        string ProviderName { get; }
        
        /// <exception cref="ArgumentNullException"></exception>
        Task<WhatAppMessageResult> SendOtpAsync(WhatsAppSendOtpParams sendParams);
        
        Task<WhatAppMessageResult> SendMessageAsync(string phoneNumber, string message);

        Task<WhatAppMessageResult> SendStatusHasChangedAsync(string phoneNumber, string eventName, string statusName, string templateName = "");
    }
}
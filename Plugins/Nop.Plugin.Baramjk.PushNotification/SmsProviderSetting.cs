using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Configuration;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.PushNotification.Services.Providers;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification
{
    public class RawSmsProviderSetting
    {
        public SmsProviderMode Mode { get; set; }
        public bool IsEnabled { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Source { get; set; }
        public string Url { get; set; }
    }

    public class FCCSmsProviderSetting
    {
        public SmsProviderMode Mode { get; set; }
        public bool IsEnabled { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Source { get; set; }
        public string Url { get; set; }
        public int AccountId { get; set; }
        public string MSISDN { get; set; }
    }

    public class SmsProviderSetting : ISettings
    {
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.IsTransactionalEnabled")]
        public bool IsTransactionalEnabled { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.TransactionalUserName")]
        public string TransactionalUserName { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.TransactionalPassword")]
        public string TransactionalPassword { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.TransactionalSource")]
        public string TransactionalSource { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.TransactionalUrl")]
        public string TransactionalUrl { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.TransactionalAccountId")]
        public int TransactionalAccountId { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.TransactionalMSISDN")]
        public string TransactionalMSISDN { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.IsPromotionalEnabled")]
        public bool IsPromotionalEnabled { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.PromotionalUserName")]
        public string PromotionalUserName { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.PromotionalPassword")]
        public string PromotionalPassword { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.PromotionalSource")]
        public string PromotionalSource { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.PromotionalUrl")]
        public string PromotionalUrl { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.PromotionalAccountId")]
        public int PromotionalAccountId { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.PromotionalMSISDN")]
        public string PromotionalMSISDN { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.SMS.Provider")]
        public string ProviderName { get; set; }

        public IList<SelectListItem> AvailableProviders { get; set; } = Enum.GetValues(typeof(SmsProviderEnum))
            .Cast<SmsProviderEnum>().Select(v =>
                new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList();

        public RawSmsProviderSetting ToRaw(SmsProviderMode mode)
        {
            switch (mode)
            {
                case SmsProviderMode.Transactional:
                    return new RawSmsProviderSetting
                    {
                        Mode = mode,
                        IsEnabled = IsTransactionalEnabled,
                        Password = TransactionalPassword,
                        Source = TransactionalSource,
                        Url = TransactionalUrl,
                        UserName = TransactionalUserName,
                    };
                case SmsProviderMode.Promotional:
                    return new RawSmsProviderSetting
                    {
                        Mode = mode,
                        IsEnabled = IsPromotionalEnabled,
                        Password = PromotionalPassword,
                        Source = PromotionalSource,
                        Url = PromotionalUrl,
                        UserName = PromotionalUserName,
                    };
            }

            return default;
        }

        public FCCSmsProviderSetting ToRawFCCSmsSetting(SmsProviderMode mode)
        {
            switch (mode)
            {
                case SmsProviderMode.Transactional:
                    return new FCCSmsProviderSetting
                    {
                        Mode = mode,
                        IsEnabled = IsTransactionalEnabled,
                        Password = TransactionalPassword,
                        Source = TransactionalSource,
                        Url = TransactionalUrl,
                        UserName = TransactionalUserName,
                        AccountId = TransactionalAccountId,
                        MSISDN = TransactionalMSISDN,
                    };
                case SmsProviderMode.Promotional:
                    return new FCCSmsProviderSetting
                    {
                        Mode = mode,
                        IsEnabled = IsPromotionalEnabled,
                        Password = PromotionalPassword,
                        Source = PromotionalSource,
                        Url = PromotionalUrl,
                        UserName = PromotionalUserName,
                        AccountId = PromotionalAccountId,
                        MSISDN = PromotionalMSISDN,
                    };
            }

            return default;
        }
    }
}
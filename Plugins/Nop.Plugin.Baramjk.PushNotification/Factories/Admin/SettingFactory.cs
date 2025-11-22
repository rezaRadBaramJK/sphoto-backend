using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.Framework.Services.PushNotification;
using Nop.Plugin.Baramjk.PushNotification.Models.WhatsApp;
using Nop.Plugin.Baramjk.PushNotification.Plugins;

namespace Nop.Plugin.Baramjk.PushNotification.Factories.Admin
{
    public class SettingFactory
    {
        private readonly WhatsAppSettings _whatsAppSettings;
        private readonly IEnumerable<IWhatsAppProvider> _whatsAppProviders;

        public SettingFactory(
            WhatsAppSettings whatsAppSettings, 
            IEnumerable<IWhatsAppProvider> whatsAppProviders)
        {
            _whatsAppSettings = whatsAppSettings;
            _whatsAppProviders = whatsAppProviders;
        }


        public WhatsAppSettingViewModel PreparedWhatsAppSettingViewModel()
        {
            var availableProviders = _whatsAppProviders
                .Select(p => new SelectListItem
                {
                    Text = p.ProviderName,
                    Value = p.ProviderName,
                    Selected = p.ProviderName == _whatsAppSettings.ProviderName
                })
                .ToList();
            if (availableProviders.Any(p => p.Selected) == false)
            {
                var a = availableProviders.FirstOrDefault();
                if (a != null)
                    a.Selected = true;
            }
            
            
            return new WhatsAppSettingViewModel
            {
                Username = _whatsAppSettings.Username,
                Password = _whatsAppSettings.Password,
                ApiSid = _whatsAppSettings.ApiSid,
                ApiSecret = _whatsAppSettings.ApiSecret,
                SenderPhoneNumber = _whatsAppSettings.SenderPhoneNumber,
                AvailableProviders = availableProviders
            };
        }
    }
}
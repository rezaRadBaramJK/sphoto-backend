using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models.WhatsApp
{
    public record WhatsAppSettingViewModel: BaseNopModel
    {
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.WhatsApp.Username")]
        public string Username { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.WhatsApp.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.WhatsApp.Provider")]
        public string ProviderName { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.WhatsApp.ApiSid")]
        public string ApiSid { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.WhatsApp.ApiSecret")]
        public string ApiSecret { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.WhatsApp.SenderPhoneNumber")]
        public string SenderPhoneNumber { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Configuration.WhatsApp.OtpTemplateName")]
        public string OtpTemplateName { get; set; }
        
        public IList<SelectListItem> AvailableProviders { get; set; }
    }
}
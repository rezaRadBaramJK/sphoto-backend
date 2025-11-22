using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models.Admin.EventNotificationConfigs
{
    public record EventNotificationConfigModel: BaseNopEntityModel
    {
        [NopResourceDisplayName("Baramjk.PushNotification.Admin.EventNotificationConfig.EventName")]
        public string EventName { get; set; }
        
        public string LocalizedEventName { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Admin.EventNotificationConfig.StatusName")]
        public string StatusName { get; set; }
        
        public string LocalizedStatusName { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Admin.EventNotificationConfig.TemplateName")]
        public string TemplateName { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Admin.EventNotificationConfig.UseSms")]
        public bool UseSms { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Admin.EventNotificationConfig.UseFirebase")]
        public bool UseFirebase { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Admin.EventNotificationConfig.UseWhatsApp")]
        public bool UseWhatsApp { get; set; }
        
        public IList<SelectListItem> AvailableEventNames { get; set; }
        
        
    }
}
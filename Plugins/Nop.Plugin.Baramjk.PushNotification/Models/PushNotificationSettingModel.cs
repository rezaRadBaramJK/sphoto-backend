using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.PushNotification.Models.WhatsApp;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public record PushNotificationSettingModel : BaseNopModel
    {
        [NopResourceDisplayName("Baramjk.PushNotification.DisableNotification")]
        public bool DisableNotification { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.Strategy")]
        public int Strategy { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.FireBaseConfig")]
        public string FireBaseConfig { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.ServerKey")]
        public string ServerKey { get; set; }
        
        [NopResourceDisplayName("Baramjk.PushNotification.PrivateKeyConfig")]
        public string PrivateKeyConfig { get; set; }
        public string SoundFileName { get; set; }
        
        public WhatsAppSettingViewModel WhatsAppSettings { get; set; }
        
        public IList<SelectListItem> StrategiesListItems { get; set; }
    }
}
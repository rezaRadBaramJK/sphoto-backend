using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public class UpdateTokenModel
    {
        [NopResourceDisplayName("Baramjk.PushNotification.Device")]
        public string Device { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Token")]
        public string Token { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Platform")]
        public NotificationPlatform Platform { get; set; }
    }
}
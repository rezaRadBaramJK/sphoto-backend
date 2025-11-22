using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public record SmsProviderSettingModel: BaseNopModel
    {
        [NopResourceDisplayName("Baramjk.PushNotification.IsEnabled")]
        public bool IsEnabled { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.UserName")]
        public string UserName { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Password")]
        public string Password { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Source")]
        public string Source { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Url")]

        public string Url { get; set; }
    }
}
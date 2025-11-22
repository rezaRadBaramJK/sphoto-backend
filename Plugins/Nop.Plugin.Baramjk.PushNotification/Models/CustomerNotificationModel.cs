using System;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public record CustomerNotificationModel : IDomainModel
    {
        public int Id { get; set; }

        [TableField(Visible = false)]
        public NotificationPlatform NotificationPlatform { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Body")]
        public string Body { get; set; }

        [TableField(RenderCustom = "RenderImage", Width = "100px")]
        [NopResourceDisplayName("Baramjk.PushNotification.Image")]
        public string Image { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.NotificationType")]
        public string NotificationType { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.PlatForm")]
        public string NotificationPlatformName => NotificationPlatform.ToString();

        [NopResourceDisplayName("Baramjk.PushNotification.OnDateTime")]
        public DateTime OnDateTime { get; set; }

        [RenderButtonView("/Admin/PushNotificationRelCustomer/List/", "Id")]
        public int View => Id;
    }
}
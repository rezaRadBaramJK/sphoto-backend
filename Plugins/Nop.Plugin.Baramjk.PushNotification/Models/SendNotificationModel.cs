using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public class SendNotificationModel
    {
        [UIHint("Picture")]
        [NopResourceDisplayName("Baramjk.PushNotification.Picture")]
        public int? PictureId { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Title")]
        public string? Title { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.NotificationType")]
        public string NotificationType { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Code")]
        public string Code { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.ExtraData")]
        public string ExtraData { get; set; }

        [NopResourceDisplayName("Baramjk.PushNotification.Platform")]
        public NotificationPlatform? Platform { get; set; }

        public List<int> CustomerIds { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.Schedule")]

        public bool Schedule { get; set; }
        [NopResourceDisplayName("Baramjk.PushNotification.OnDateTime")]

        public DateTime OnDateTime { get; set; }
    }
}
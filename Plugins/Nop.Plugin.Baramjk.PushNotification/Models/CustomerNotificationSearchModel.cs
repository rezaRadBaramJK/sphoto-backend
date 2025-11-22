using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public class CustomerNotificationSearchModel : ExtendedSearchModel
    {
        public int CustomerId { get; set; }
        public NotificationPlatform Platform { get; set; }
    }
}
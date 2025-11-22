using Nop.Core;

namespace Nop.Plugin.Baramjk.PushNotification.Domain
{
    public class PushNotificationItemRelCustomer : BaseEntity
    {
        public int PushNotificationItemId { get; set; }
        public int CustomerId { get; set; }
        public string Token { get; set; }
        public bool IsRead { get; set; }
    }
}
using Nop.Core;

namespace Nop.Plugin.Baramjk.PushNotification.Domain
{
    public class CustomerNotifyProfileEntity: BaseEntity
    {
        public int CustomerId { get; set; }
        
        public bool Sale { get; set; }
        
        public bool Discount { get; set; }
        
        public bool BackInStock { get; set; }
    }
}
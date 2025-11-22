using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PushNotification.Models
{
    public class CustomerNotifyProfileDto : CamelCaseBaseDto
    {
        public bool Sale { get; set; }
        
        public bool Discount { get; set; }
        
        public bool BackInStock { get; set; }
    }
}
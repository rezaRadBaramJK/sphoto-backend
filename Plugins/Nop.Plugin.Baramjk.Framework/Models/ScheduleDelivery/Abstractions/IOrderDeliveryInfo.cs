using Nop.Plugin.Baramjk.Framework.Models.ScheduleDelivery.Dto;

namespace Nop.Plugin.Baramjk.Framework.Models.ScheduleDelivery.Abstractions
{
    public interface IOrderDeliveryInfo
    {
        public int OrderId { get; }
        
        public OrderDeliveryInfoDto DeliveryInfo { get; set; }
        
    }
}
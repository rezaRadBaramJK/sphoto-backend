using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models.ScheduleDelivery.Dto;

namespace Nop.Plugin.Baramjk.Framework.Models.ScheduleDelivery.Abstractions
{
    public interface IScheduleDeliveryDtoBase
    {
        public List<ScheduleDeliveryDto> Deliveries { get; set; }
        
        public int VendorId { get; set; }
    }
}
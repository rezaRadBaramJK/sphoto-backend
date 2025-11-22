using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Models.ScheduleDelivery.Dto
{
    public class OrderDeliveryInfoDto: CamelCaseModelDto
    {
        public string Date { get; set; }
        
        public string FromTime { get; set; }
        
        public string ToTime { get; set; }
    }
}
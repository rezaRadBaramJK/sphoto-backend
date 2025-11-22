using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Models.ScheduleDelivery.Dto
{
    public class ScheduleDeliveryDto: CamelCaseModelWithIdDto
    {
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public decimal Price { get; set; }
        
        public string PriceString { get; set; }
        
        public decimal UpperOrderPriceDiscount { get; set; }
        
        public int Capacity { get; set; }
        
        public bool IsPickup { get; set; }
        
        public bool IsEnable { get; set; }
        
        public string Type { get; set; }
        
        public string DefaultFromAvailableTime { get; set; }
        
        public string DefaultToAvailableTime { get; set; }
        
        public int VendorId { get; set; }
        
        public int Duration { get; set; }
        
        public string LogoUrl { get; set; }
        
        public int PictureId { get; set; }
        
    }
}
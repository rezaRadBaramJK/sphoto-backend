using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reservations;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Order
{
    public class OrderDetailDto : CamelCaseModelWithIdDto
    {
        public string OrderGuid { get; set; }
        public DateTime CreatedOn { get; set; }
        
        public string OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string PhoneNumber { get; set; }
        
        public string PaymentStatus { get; set; }
        public string OrderSubtotal { get; set; }
        public string OrderSubTotalDiscount { get; set; }
        public string OrderTotalDiscount { get; set; }
        public string OrderTotal { get; set; }
        public List<ReservationItemDto> Reservations { get; set; }
    }
}
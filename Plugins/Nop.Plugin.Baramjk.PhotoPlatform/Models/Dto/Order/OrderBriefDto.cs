using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Reservations;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Order
{
    public class OrderBriefDto : CamelCaseModelWithIdDto
    {
        public Guid OrderGuid { get; set; }
        public string CreatedOn { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public List<ReservationItemDto> Reservations { get; set; }
    }
}
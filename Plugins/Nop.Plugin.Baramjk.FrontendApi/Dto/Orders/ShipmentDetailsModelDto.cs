using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Orders
{
    public class ShipmentDetailsModelDto : ModelWithIdDto
    {
        public string TrackingNumber { get; set; }

        public string TrackingNumberUrl { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public IList<ShipmentStatusEventModelDto> ShipmentStatusEvents { get; set; }

        public bool ShowSku { get; set; }

        public IList<ShipmentItemModelDto> Items { get; set; }

        public OrderDetailsModelDto Order { get; set; }
    }
}
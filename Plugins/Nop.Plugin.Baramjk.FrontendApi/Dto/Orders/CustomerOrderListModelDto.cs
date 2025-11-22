using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Orders
{
    public class CustomerOrderListModelDto : ModelDto
    {
        public IList<CustomerOrderDetailsModelDto> Orders { get; set; } = new List<CustomerOrderDetailsModelDto>();

        public IList<RecurringOrderModelDto> RecurringOrders { get; set; } = new List<RecurringOrderModelDto>();

        public IList<string> RecurringPaymentErrors { get; set; } = new List<string>();
        
        #region Nested Classes

        public class CustomerOrderDetailsModelDto : ModelWithIdDto
        {
            public string CustomOrderNumber { get; set; }

            public string OrderTotal { get; set; }

            public bool IsReturnRequestAllowed { get; set; }

            public int OrderStatusEnum { get; set; }

            public string OrderStatus { get; set; }
            public int OrderStatusId { get; set; }

            public string PaymentStatus { get; set; }
            public int PaymentStatusId { get; set; }

            public string ShippingStatus { get; set; }
            public int ShippingStatusId { get; set; }

            public PictureModel FirstProductPicture { get; set; }

            public DateTime CreatedOn { get; set; }

            public IList<OrderDetailsModelDto.OrderItemModelDto> Items { get; set; } =
                new List<OrderDetailsModelDto.OrderItemModelDto>();
        }

        #endregion
    }
}
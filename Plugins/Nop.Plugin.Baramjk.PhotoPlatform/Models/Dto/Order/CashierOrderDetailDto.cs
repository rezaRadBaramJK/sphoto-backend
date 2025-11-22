using System.Collections.Generic;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.Api.Cashier.Order;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Order
{
    public class CashierOrderDetailDto : OrderDetailDto
    {
        public string CashierName { get; set; }

        public List<int> CashierRefundOptionsIds { get; set; } = new() { (int)CashierRefundMethod.Cash };
    }
}
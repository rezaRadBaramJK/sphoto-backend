using System;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.ChartReportServices
{
    public class OrderReportRequest
    {
        public Period Period { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
    }
}
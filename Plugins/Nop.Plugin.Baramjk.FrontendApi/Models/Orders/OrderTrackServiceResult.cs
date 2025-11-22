using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Baramjk.FrontendApi.Models.Orders
{
    public class OrderTrackServiceResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Order Order { get; set; }
    }
}
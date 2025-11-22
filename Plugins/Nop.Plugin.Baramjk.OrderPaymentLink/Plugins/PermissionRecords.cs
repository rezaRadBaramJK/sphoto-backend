using Nop.Core.Domain.Security;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.ImplementNopPlugin
{
    public class PermissionRecords
    {
        public static readonly PermissionRecord ReportOrderPaymentLink = new()
        {
            Name = "ReportOrderPaymentLink", SystemName = "ReportOrderPaymentLink", Category = "Orders"
        };

        public static readonly PermissionRecord CreateOrderPaymentLink = new()
        {
            Name = "CreateOrderPaymentLink", SystemName = "CreateOrderPaymentLink", Category = "Orders"
        };
    }
}
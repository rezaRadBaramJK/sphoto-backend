namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models
{
    public class SendRefundRequest
    {
        public string KeyType { get; set; }
        public string Key { get; set; }
        public bool RefundChargeOnCustomer { get; set; }
        public bool ServiceChargeOnCustomer { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
        public int AmountDeductedFromSupplier { get; set; }
    }
}
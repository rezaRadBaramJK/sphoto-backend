namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus
{
    public class GetPaymentStatusSupplier
    {
        public int SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int InvoiceShare { get; set; }
        public int ProposedShare { get; set; }
        public int DepositShare { get; set; }
    }
}
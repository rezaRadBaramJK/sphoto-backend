namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models
{
    public class Invoiceitem : IBaseResponse
    {
        public string ItemName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }

}
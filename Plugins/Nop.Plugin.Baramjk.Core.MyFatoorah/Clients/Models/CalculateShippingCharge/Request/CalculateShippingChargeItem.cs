namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.CalculateShippingCharge.Request
{
    public class CalculateShippingChargeItem
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Weight { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
    }
}
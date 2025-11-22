namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers
{
    public class GetSupplierCodeResult
    {
        public bool Success { get; set; }
        
        public int SupplierCode { get; set; }

        public static GetSupplierCodeResult GetSuccessfulResult(int supplierCode) => new()
        {
            Success = true,
            SupplierCode = supplierCode
        };

        public static GetSupplierCodeResult GetFailedResult() => new();
    }
}
namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.CustomerVendor
{
    public class AddVendorResponse
    {
        public int VendorId { get; set; }
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ErrorMessage { get; set; }
    }
}
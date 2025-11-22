using Nop.Plugin.Baramjk.ContactUs.Domains;

namespace Nop.Plugin.Baramjk.ContactUs.Models.Services.Payments
{
    public class GetPaymentUrlServiceParams
    {
        public decimal Price { get; set; }
        
        public string CustomerFirstName { get; set; }
        
        public string CustomerLastName {get; set;}
        
        public string CustomerPhoneNumber { get; set; }
        
        public string CustomerEmail { get; set; }
        
        public int ContactInfoId { get; set; }
        
    }
}
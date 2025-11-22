using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class CheckoutRedirectResponse : BaseDto
    {
        public string RedirectToMethod { get; set; }

        public int? Id { get; set; }
        
        public decimal OrderTotal { get; set; }
    }
}
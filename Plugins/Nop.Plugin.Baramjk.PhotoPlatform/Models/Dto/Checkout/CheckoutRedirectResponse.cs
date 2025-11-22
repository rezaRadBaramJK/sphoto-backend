using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Checkout
{
    public class CheckoutRedirectResponse : CamelCaseBaseDto
    {
        public string RedirectToMethod { get; set; }

        public int? Id { get; set; }
    }
}
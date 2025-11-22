using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Checkout
{
    public class CheckoutConfirmModelDto : CamelCaseBaseDto
    {
        public bool TermsOfServiceOnOrderConfirmPage { get; set; }
        public bool TermsOfServicePopup { get; set; }
        public string MinOrderTotalWarning { get; set; }

        public IList<string> Warnings { get; set; }
    }
}
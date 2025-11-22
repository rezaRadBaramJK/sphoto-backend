using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class GiftCardDto : ModelWithIdDto
    {
        public string CouponCode { get; set; }

        public string Amount { get; set; }

        public string Remaining { get; set; }
    }
}
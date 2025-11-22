using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class GiftCardBoxModelDto : ModelDto
    {
        public bool Display { get; set; }

        public string Message { get; set; }

        public bool IsApplied { get; set; }
    }
}
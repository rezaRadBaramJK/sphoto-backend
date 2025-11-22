using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class CheckGiftCardBalanceModelDto : ModelDto
    {
        public string Result { get; set; }

        public string Message { get; set; }

        public string GiftCardCode { get; set; }
    }
}
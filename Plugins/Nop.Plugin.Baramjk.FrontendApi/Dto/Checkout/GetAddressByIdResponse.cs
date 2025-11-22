using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout
{
    public class GetAddressByIdResponse : BaseDto
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
    }
}
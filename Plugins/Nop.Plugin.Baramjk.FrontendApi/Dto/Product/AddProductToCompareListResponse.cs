using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class AddProductToCompareListResponse : BaseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }
    }
}
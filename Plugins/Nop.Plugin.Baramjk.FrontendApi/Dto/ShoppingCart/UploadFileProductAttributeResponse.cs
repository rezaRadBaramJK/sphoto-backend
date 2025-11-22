using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart
{
    public class UploadFileProductAttributeResponse : BaseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string DownloadUrl { get; set; }

        public string DownloadGuid { get; set; }
    }
}
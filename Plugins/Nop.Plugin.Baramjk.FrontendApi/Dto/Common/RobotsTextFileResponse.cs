using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Common
{
    public class RobotsTextFileResponse : BaseDto
    {
        public string RobotsFileContent { get; set; }

        public string MimeType { get; set; }
    }
}
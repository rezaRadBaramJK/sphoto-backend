using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Common
{
    public class SitemapXmlResponse : BaseDto
    {
        public string SiteMapXML { get; set; }

        public string MimeType { get; set; }
    }
}
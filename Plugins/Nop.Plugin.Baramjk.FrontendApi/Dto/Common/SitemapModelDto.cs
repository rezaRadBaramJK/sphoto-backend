using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Common
{
    public class SitemapModelDto : ModelDto
    {
        public List<SitemapItemModelDto> Items { get; set; }

        public SitemapPageModelDto PageModel { get; set; }
    }
}
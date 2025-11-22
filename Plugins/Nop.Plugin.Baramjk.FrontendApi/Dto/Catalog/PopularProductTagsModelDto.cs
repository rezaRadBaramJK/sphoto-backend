using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog
{
    public class PopularProductTagsModelDto : ModelDto
    {
        public int TotalTags { get; set; }

        public List<ProductTagModelDto> Tags { get; set; }
    }
}
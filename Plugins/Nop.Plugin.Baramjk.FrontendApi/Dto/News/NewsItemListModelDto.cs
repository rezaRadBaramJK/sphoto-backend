using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.News
{
    public class NewsItemListModelDto : ModelDto
    {
        public int WorkingLanguageId { get; set; }

        public BasePageableModelDto PagingFilteringContext { get; set; }

        public IList<NewsItemModelDto> NewsItems { get; set; }
    }
}
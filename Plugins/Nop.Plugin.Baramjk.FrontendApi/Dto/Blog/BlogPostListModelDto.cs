using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Blog
{
    public class BlogPostListModelDto : ModelDto
    {
        public int WorkingLanguageId { get; set; }

        public BlogPagingFilteringModelDto PagingFilteringContext { get; set; }

        public IList<BlogPostModelDto> BlogPosts { get; set; }
    }
}
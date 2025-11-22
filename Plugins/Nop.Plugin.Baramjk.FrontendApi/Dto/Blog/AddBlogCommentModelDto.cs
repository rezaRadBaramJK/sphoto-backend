using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Blog
{
    public class AddBlogCommentModelDto : ModelWithIdDto
    {
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
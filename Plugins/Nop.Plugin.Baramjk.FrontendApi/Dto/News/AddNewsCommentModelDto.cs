using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.News
{
    public class AddNewsCommentModelDto : ModelDto
    {
        public string CommentTitle { get; set; }

        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
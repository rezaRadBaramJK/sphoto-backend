using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Boards
{
    public class ForumWatchResponse : BaseDto
    {
        public bool Subscribed { get; set; }

        public string Text { get; set; }

        public bool Error { get; set; }
    }
}
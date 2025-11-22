using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Boards
{
    public class ForumRowModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public string Description { get; set; }

        public int NumTopics { get; set; }

        public int NumPosts { get; set; }

        public int LastPostId { get; set; }
    }
}
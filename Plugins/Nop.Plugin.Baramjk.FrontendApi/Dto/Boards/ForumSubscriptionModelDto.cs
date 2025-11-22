using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Boards
{
    public class ForumSubscriptionModelDto : ModelWithIdDto
    {
        public int ForumId { get; set; }

        public int ForumTopicId { get; set; }

        public bool TopicSubscription { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }
    }
}
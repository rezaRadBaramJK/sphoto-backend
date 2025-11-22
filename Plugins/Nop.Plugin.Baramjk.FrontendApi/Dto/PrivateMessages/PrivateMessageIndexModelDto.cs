using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.PrivateMessages
{
    public class PrivateMessageIndexModelDto : ModelDto
    {
        public int InboxPage { get; set; }

        public int SentItemsPage { get; set; }

        public bool SentItemsTabSelected { get; set; }
    }
}
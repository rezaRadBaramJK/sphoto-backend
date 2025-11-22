using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Boards
{
    public class PostVoteResponse : BaseDto
    {
        public string Error { get; set; }

        public int VoteCount { get; set; }

        public bool IsUp { get; set; }
    }
}
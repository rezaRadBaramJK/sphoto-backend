using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Profiles
{
    public class ProfileIndexModelDto : ModelDto
    {
        public int CustomerProfileId { get; set; }

        public string ProfileTitle { get; set; }

        public int PostsPage { get; set; }

        public bool PagingPosts { get; set; }

        public bool ForumsEnabled { get; set; }
    }
}
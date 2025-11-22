using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Boards
{
    public class CustomerForumSubscriptionsModelDto : ModelDto
    {
        public IList<ForumSubscriptionModelDto> ForumSubscriptions { get; set; }

        public PagerModelDto PagerModel { get; set; }
    }
}
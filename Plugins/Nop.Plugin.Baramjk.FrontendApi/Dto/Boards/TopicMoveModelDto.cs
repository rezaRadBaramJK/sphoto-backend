using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Boards
{
    public class TopicMoveModelDto : ModelWithIdDto
    {
        public int ForumSelected { get; set; }

        public string TopicSeName { get; set; }

        public IEnumerable<SelectListItemDto> ForumList { get; set; }
    }
}
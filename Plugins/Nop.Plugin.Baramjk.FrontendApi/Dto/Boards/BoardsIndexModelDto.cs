using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Boards
{
    public class BoardsIndexModelDto : ModelDto
    {
        public IList<ForumGroupModelDto> ForumGroups { get; set; }
    }
}
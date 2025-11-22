using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Dto.Event
{
    public class CategorizedEventBriefDto : CamelCaseBaseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<EventBriefDto> Events { get; set; }
    }
}
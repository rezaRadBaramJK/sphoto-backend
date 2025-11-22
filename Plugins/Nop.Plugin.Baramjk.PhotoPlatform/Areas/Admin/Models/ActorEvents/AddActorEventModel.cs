using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEvents
{
    public class AddActorEventModel
    {
        public List<int> ActorIds { get; set; }
        public int EventId { get; set; }
    }
}
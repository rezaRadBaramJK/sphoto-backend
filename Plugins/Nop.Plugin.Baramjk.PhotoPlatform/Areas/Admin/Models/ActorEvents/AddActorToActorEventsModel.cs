using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEvents
{
    public record AddActorToActorEventsModel : BaseNopModel
    {
        public AddActorToActorEventsModel()
        {
            SelectedActorIds = new List<int>();
        }


        public int EventId { get; set; }
        public IList<int> SelectedActorIds { get; set; }
    }
}
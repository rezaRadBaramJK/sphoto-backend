using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.SupervisorEvents
{
    public class AddSupervisorEventModel
    {
        public List<int> CustomerIds { get; set; }
        public int EventId { get; set; }
    }
}
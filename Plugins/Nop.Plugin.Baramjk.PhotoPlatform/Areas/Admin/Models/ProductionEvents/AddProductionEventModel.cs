using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ProductionEvents
{
    public class AddProductionEventModel
    {
        public List<int> CustomerIds { get; set; }
        public int EventId { get; set; }
    }
}
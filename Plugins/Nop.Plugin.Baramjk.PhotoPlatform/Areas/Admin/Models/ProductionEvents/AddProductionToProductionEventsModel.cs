using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ProductionEvents
{
    public record AddProductionToProductionEventsModel : BaseNopModel
    {
        public AddProductionToProductionEventsModel()
        {
            SelectedCustomerIds = new List<int>();
        }


        public int EventId { get; set; }
        public IList<int> SelectedCustomerIds { get; set; }
    }
}
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.CashierEvents
{
    public class AddCashierEventModel
    {
        public List<int> CustomerIds { get; set; }
        public int EventId { get; set; }
    }
}
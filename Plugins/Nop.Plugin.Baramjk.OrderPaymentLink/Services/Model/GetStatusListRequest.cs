using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Services.Model
{
    public class GetStatusListRequest
    {
        public List<int> OrderIds { get; set; }
    }

    public class GetStatusListResponse
    {
        public int OrderId { get; set; }

        public bool Status { get; set; }
    }
}
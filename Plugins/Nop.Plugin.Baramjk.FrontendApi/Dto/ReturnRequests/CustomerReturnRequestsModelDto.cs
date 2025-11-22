using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Orders;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ReturnRequests
{
    public class CustomerReturnRequestsModelDto : ModelDto
    {
        public IList<ReturnRequestModelDto> Items { get; set; }
    }
}
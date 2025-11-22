using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Models.Orders;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Orders
{
    public class ReOrderDto
    {
        public bool Success { get; set; }
        
        public string Message { get; set; }
        
        public List<ReOrderProductDto> FailedProducts { get; set; } = new();
        
        public List<ReOrderProductDto> AddedProducts { get; set; } = new();
        
        
    }
}
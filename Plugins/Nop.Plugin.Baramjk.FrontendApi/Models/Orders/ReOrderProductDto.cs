using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Models.Orders
{
    public class ReOrderProductDto: ModelWithIdDto
    {
        public string Name { get; set; }
        
        public string ImageUrl { get; set; }
    }
}
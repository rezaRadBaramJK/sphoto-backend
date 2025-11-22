using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Models.BuyXGetY.Dto
{
    public class BuyXGetYItemDto: CamelCaseModelWithIdDto
    {
        public string Name { get; set; }
        
        public int BuyCount { get; set; }
        
        public int GetCount { get; set; }
        
        public bool IsActive { get; set; }
        
        public int DisplayOrder { get; set; }
    }
}
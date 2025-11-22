using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Models.BuyXGetY.Dto
{
    public class BuyXGetYDto : CamelCaseModelDto
    {
        public IList<BuyXGetYItemDto> Items { get; set; } = new List<BuyXGetYItemDto>();
    }
}
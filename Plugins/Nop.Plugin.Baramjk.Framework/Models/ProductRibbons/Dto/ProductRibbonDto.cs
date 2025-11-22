using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Models.ProductRibbons.Dto
{
    public class ProductRibbonDto: CamelCaseModelDto
    {
        public IList<RibbonDto> Ribbons { get; set; } = new List<RibbonDto>();
    }
}
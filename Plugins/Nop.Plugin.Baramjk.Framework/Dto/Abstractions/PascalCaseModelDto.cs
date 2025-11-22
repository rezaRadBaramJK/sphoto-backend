using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Dto.Abstractions
{
    public class PascalCaseModelDto: PascalCaseBaseDto, ICustomProperties
    {
        public Dictionary<string, object> CustomProperties { get; set; } = new();
    }
}
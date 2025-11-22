using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Dto.Abstractions
{
    public abstract class CamelCaseModelDto : CamelCaseBaseDto, ICustomProperties
    {
        
        public Dictionary<string, object> CustomProperties { get; set; } = new();
        
    }
}
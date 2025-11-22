using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Dto.Abstractions
{
    public abstract class ModelDto: BaseDto, ICustomProperties
    {
        public Dictionary<string, object> CustomProperties { get; set; } = new();
    }
}
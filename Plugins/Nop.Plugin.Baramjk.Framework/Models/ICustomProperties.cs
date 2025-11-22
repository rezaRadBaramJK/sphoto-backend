using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Models
{
    public interface ICustomProperties
    {
        public Dictionary<string, object> CustomProperties { get; set; }
    }
}
using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Models
{
    public class DtoBase : IDtoWithId, ICustomProperties
    {
        public int Id { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; }

        public void AddCustomProperty(string key, object value)
        {
            CustomProperties ??= new Dictionary<string, object>();
            CustomProperties[key] = value;
        }
    }
}
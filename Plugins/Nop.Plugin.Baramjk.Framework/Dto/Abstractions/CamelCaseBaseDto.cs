using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nop.Plugin.Baramjk.Framework.Dto.Abstractions
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public abstract class CamelCaseBaseDto
    {
        
    }
}
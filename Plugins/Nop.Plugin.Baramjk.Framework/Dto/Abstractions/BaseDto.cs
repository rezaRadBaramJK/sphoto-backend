using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nop.Plugin.Baramjk.Framework.Dto.Abstractions
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public abstract class BaseDto
    {
        
    }
}
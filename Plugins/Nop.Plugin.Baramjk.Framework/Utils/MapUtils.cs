using Newtonsoft.Json;

namespace Nop.Plugin.Baramjk.Framework.Utils
{
    public class MapUtils
    {
        private static readonly JsonSerializerSettings _serializerSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static TTo Map<TTo>(object data, JsonSerializerSettings settings = null)
        {
            if (data == null)
                return default;

            settings ??= _serializerSettings;

            var json = JsonConvert.SerializeObject(data, settings);
            var deserializeObject = JsonConvert.DeserializeObject<TTo>(json);
            return deserializeObject;
        }
    }
}
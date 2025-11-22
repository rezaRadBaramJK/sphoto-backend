namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class ObjectEx
    {
        public static string ToJson(this object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public static T DeserializeFromJson<T>(this string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
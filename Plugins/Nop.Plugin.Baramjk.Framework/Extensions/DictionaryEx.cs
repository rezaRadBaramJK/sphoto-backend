using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class DictionaryEx
    {
        public static void AddSafe<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value,
            bool replace = true)
        {
            if (dic == null)
                return;

            if (replace)
            {
                dic[key] = value;
                return;
            }

            if (dic.ContainsKey(key))
                return;

            dic[key] = value;
        }
    }
}
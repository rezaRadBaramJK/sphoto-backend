using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Core.Extensions
{
    public static class ArrayExtensions
    {

        public static IList<int> ToIntArray(this string text, string separator = ",")
        {
            if (string.IsNullOrEmpty(text))
                return new List<int>();
            
            var array = text.Split(separator);
            var results = new List<int>();
            foreach (var currentItem in array)
            {
                if(int.TryParse(currentItem, out var result))
                    results.Add(result);
            }

            return results;
        }
    }
}
using System;
using System.Linq;

namespace Nop.Plugin.Baramjk.BackendApi.Helpers
{
    public static class StringExtensions
    {
        public static int[] ToIdArray(this string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return Array.Empty<int>();

            var rezArray = ids.Split(";")
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse).ToArray();

            return rezArray;
        }
    }
}
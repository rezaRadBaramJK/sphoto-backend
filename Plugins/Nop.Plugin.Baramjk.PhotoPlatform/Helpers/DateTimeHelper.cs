using System;
using System.Linq;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Helpers
{
    public static class DateTimeHelper
    {
        
        public static DateTime[] GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            return Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset))
                .ToArray();
        }
        
    }
}
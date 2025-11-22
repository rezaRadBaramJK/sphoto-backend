using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class ModelsEx
    {
        public static void AutoIncrement(this IEnumerable<IDomainModel> models)
        {
            var i = 1;
            foreach (var item in models)
                item.Id = i++;
        }
    }
}
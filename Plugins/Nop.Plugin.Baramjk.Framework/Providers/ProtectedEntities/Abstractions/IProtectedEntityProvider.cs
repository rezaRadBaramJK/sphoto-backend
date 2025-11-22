using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Providers.ProtectedEntities.Models;

namespace Nop.Plugin.Baramjk.Framework.Providers.ProtectedEntities.Abstractions
{
    public interface IProtectedEntityProvider
    {
        public string EntityName { get; }

        public string ListUrl { get; }
        public string ViewUrl { get; }
        public bool HasView { get; }
        public bool HasPicture { get; }

        public Task<IList<ProtectedEntityItem>> PrepareItemsAsync(int[] itemIds);
    }
}
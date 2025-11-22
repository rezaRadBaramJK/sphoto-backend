using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Utils;
#pragma warning disable CS1998

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public class BaseDomainFactory<TDomain, TDomainModel> : IDomainFactory<TDomain, TDomainModel>
        where TDomainModel : IDomainModel, new()
        where TDomain : BaseEntity, new()
    {
        public virtual async Task<TDomainModel> GetModelAsync(TDomain entity)
        {
            var model = MapUtils.Map<TDomainModel>(entity);
            return model;
        }

        public virtual async Task<List<TDomainModel>> GetModelsAsync(IEnumerable<TDomain> entities)
        {
            return await entities.SelectAwait(async (m) => await GetModelAsync(m)).ToListAsync();
        }
    }
}
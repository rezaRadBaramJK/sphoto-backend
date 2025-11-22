using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public interface IDomainFactory<in TDomain, TDomainModel>
        where TDomainModel : IDomainModel, new()
        where TDomain : new()
    {
        Task<TDomainModel> GetModelAsync(TDomain entity);
        Task<List<TDomainModel>> GetModelsAsync(IEnumerable<TDomain> entities);
    }
}
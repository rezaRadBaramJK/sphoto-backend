using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.DomainServices
{
    public interface IDomainService<TDomain, in TDomainModel>
        where TDomainModel : IDomainModel, new()
        where TDomain : new()
    {
        Task<TDomain> AddAsync(TDomainModel model);
        Task<TDomain> GetByIdAsync(int id);
        Task<TDomain> EditAsync(TDomainModel model);
        Task<TDomain> DeleteAsync(int id);
        Task<List<TDomain>> GetListAsync();
        Task<TDomain> AddAsync(TDomain entity);
        Task<TDomain> EditAsync(TDomain entity, bool checkExist = false);
        Task<List<TDomain>> GetListAsync(IPagingRequestModel pagingRequestModel);
        IQueryable<TDomain> GetQueryable(bool descending = true);
    }
}
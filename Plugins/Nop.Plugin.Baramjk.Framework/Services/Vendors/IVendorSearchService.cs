using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Vendors;

namespace Nop.Plugin.Baramjk.Framework.Services.Vendors
{
    public interface IVendorSearchService
    {
        Task<List<Vendor>> GetAllVendorsAsync(string name = "", string email = "",
            Dictionary<string, string> genericAttribute = null, Dictionary<int, string> attribute = null,
            int? categoryId = 0, List<int> ids = null, int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false);

        Task<List<int>> GetVendorIdsByCategoryIdAsync(int categoryId, int count);
    }
}
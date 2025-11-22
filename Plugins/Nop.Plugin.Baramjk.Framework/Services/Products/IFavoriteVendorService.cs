using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public interface IFavoriteVendorService
    {
        Task<FavoriteVendor> AddAsync(int customerId, int vendorId);
        Task<FavoriteVendor> DeleteAsync(int customerId, int vendorId);
        Task<int> GetFansCountAsync(int vendorId);
        Task<List<int>> GetCustomerFavoriteVendorsAsync(int customerId);
        Task<bool> IsFavoriteAsync(int customerId, int vendorId);
        Task<List<FavoriteVendor>> DeleteAllAsync(int customerId);
    }
}
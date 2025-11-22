using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public class FavoriteVendorService : IFavoriteVendorService
    {
        private readonly IRepository<FavoriteVendor> _repositoryFavoriteVendor;

        public FavoriteVendorService(IRepository<FavoriteVendor> repositoryFavoriteVendor)
        {
            _repositoryFavoriteVendor = repositoryFavoriteVendor;
        }

        public async Task<FavoriteVendor> AddAsync(int customerId, int vendorId)
        {
            var favoriteVendor = new FavoriteVendor
            {
                CustomerId = customerId,
                VendorId = vendorId
            };

            await _repositoryFavoriteVendor.InsertAsync(favoriteVendor);
            return favoriteVendor;
        }

        public async Task<FavoriteVendor> DeleteAsync(int customerId, int vendorId)
        {
            var favoriteVendor = await _repositoryFavoriteVendor.Table.FirstOrDefaultAsync(item =>
                item.CustomerId == customerId && item.VendorId == vendorId);

            if (favoriteVendor == null)
                return null;

            await _repositoryFavoriteVendor.DeleteAsync(favoriteVendor);
            return favoriteVendor;
        }

        public async Task<List<FavoriteVendor>> DeleteAllAsync(int customerId)
        {
            var favoriteVendors = await _repositoryFavoriteVendor.Table
                .Where(item => item.CustomerId == customerId)
                .ToListAsync();

            if (favoriteVendors == null)
                return null;

            await _repositoryFavoriteVendor.DeleteAsync(favoriteVendors);
            return favoriteVendors;
        }

        public async Task<List<int>> GetCustomerFavoriteVendorsAsync(int customerId)
        {
            return await _repositoryFavoriteVendor.Table
                .Where(item => item.CustomerId == customerId)
                .Select(item => item.VendorId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> IsFavoriteAsync(int customerId, int vendorId)
        {
            return await _repositoryFavoriteVendor.Table.AnyAsync(item =>
                item.CustomerId == customerId && item.VendorId == vendorId);
        }

        public async Task<int> GetFansCountAsync(int vendorId)
        {
            var count = await _repositoryFavoriteVendor.Table
                .CountAsync(item => item.VendorId == vendorId);

            return count;
        }
    }
}
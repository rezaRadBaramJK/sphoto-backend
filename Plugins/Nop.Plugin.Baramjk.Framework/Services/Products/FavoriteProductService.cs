using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using System.Linq;
using LinqToDB;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public class FavoriteProductService : IFavoriteProductService
    {
        private readonly IProductDtoFactory _productDtoFactory;
        private readonly IRepository<FavoriteProduct> _repositoryFavoriteProduct;

        public FavoriteProductService(IRepository<FavoriteProduct> repositoryFavoriteProduct,
            IProductDtoFactory productDtoFactory)
        {
            _repositoryFavoriteProduct = repositoryFavoriteProduct;
            _productDtoFactory = productDtoFactory;
        }

        public async Task<FavoriteProduct> AddAsync(int customerId, int productId)
        {
            var favoriteVendor = await GetFavoriteProductAsync(customerId, productId);
            if (favoriteVendor != null)
                return favoriteVendor;

            favoriteVendor = new FavoriteProduct
            {
                CustomerId = customerId,
                ProductId = productId
            };

            await _repositoryFavoriteProduct.InsertAsync(favoriteVendor);
            return favoriteVendor;
        }

        public async Task<FavoriteProduct> DeleteAsync(int customerId, int productId)
        {
            var favoriteVendor = await GetFavoriteProductAsync(customerId, productId);

            if (favoriteVendor == null)
                return null;

            await _repositoryFavoriteProduct.DeleteAsync(favoriteVendor);
            return favoriteVendor;
        }

        public async Task<List<FavoriteProduct>> DeleteAsync(int customerId)
        {
            var favoriteProducts = await GetFavoriteProductsAsync(customerId);

            if (favoriteProducts == null)
                return null;

            await _repositoryFavoriteProduct.DeleteAsync(favoriteProducts);
            return favoriteProducts;
        }

        public async Task<IEnumerable<ProductOverviewDto>> GetFavoriteProductModelsAsync(int customerId)
        {
            var ids = await _repositoryFavoriteProduct.Table
                .Where(item => item.CustomerId == customerId)
                .Select(item => item.ProductId)
                .ToArrayAsync();

            var overviewModels = await _productDtoFactory.PrepareProductOverviewAsync(ids);
            return overviewModels;
        }

        public async Task<List<FavoriteProduct>> GetFavoriteProductsAsync(int customerId)
        {
            var favoriteVendor = await _repositoryFavoriteProduct.Table
                .Where(item => item.CustomerId == customerId)
                .ToListAsync();

            return favoriteVendor;
        }

        private async Task<FavoriteProduct> GetFavoriteProductAsync(int customerId, int productId)
        {
            var favoriteVendor = await _repositoryFavoriteProduct.Table.FirstOrDefaultAsync(item =>
                item.CustomerId == customerId && item.ProductId == productId);
            return favoriteVendor;
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Models.Products;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public interface IFavoriteProductService
    {
        Task<FavoriteProduct> AddAsync(int customerId, int productId);
        Task<FavoriteProduct> DeleteAsync(int customerId, int productId);
        Task<List<FavoriteProduct>> DeleteAsync(int customerId);
        Task<IEnumerable<ProductOverviewDto>> GetFavoriteProductModelsAsync(int customerId);
        Task<List<FavoriteProduct>> GetFavoriteProductsAsync(int customerId);
    }
}
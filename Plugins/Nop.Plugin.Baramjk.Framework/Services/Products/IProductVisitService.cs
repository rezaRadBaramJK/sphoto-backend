using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Models.Products;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public interface IProductVisitService
    {
        Task<List<ProductVisitDto>> GetProductsVisitAsync(IEnumerable<int> productIds);
        Task<List<ProductVisitDto>> GetProductsMostVisitedAsync(int pageNumber = 0, int pageSize = 25);
        Task<List<ProductVisitDto>> GetProductsMostVisitedAsync(int vendorId, int pageNumber = 0, int pageSize = 25);

        Task UpdateVisit(int productId, int count);
        Task InsertVisitAsync(int productId);
        Task<int> IncreaseVisitAsync(int productId);
        Task<int> GetVisitAsync(int productId);
    }
}
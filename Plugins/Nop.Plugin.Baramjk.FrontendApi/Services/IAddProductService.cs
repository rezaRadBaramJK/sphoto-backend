using System.Threading.Tasks;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.AddProductServices;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public interface IAddProductService
    {
        Task<AddProductResponse> AddProductAsync(ProductDataModel dataModel);
        Task<AddProductResponse> EditProductAsync(int id, ProductDataModel dataModel);
    }
}
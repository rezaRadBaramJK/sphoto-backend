using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking
{
    public interface IBookingDateProductAttributeService
    {
        Task<ProductAttributeMapping> GetOrCreateSelectDateAttributeAsync(int productId);
        Task<ProductAttributeMapping> DeleteDateAttributeAsync(int productId);
    }
}
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Orders;
using Nop.Plugin.Baramjk.FrontendApi.Models.Orders;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public interface IWebApiOrderModelFactory
    {
        Task<CustomerOrderListModelDto> CustomerOrdersSync(bool withFirstProductPicture = false,
            int? orderStatus = null);

        Task<OrderDetailsModelDto> PrepareOrderDetailsModelAsync(Order order);
        Task<CustomerOrderListModelDto> VendorOrdersSync(bool includeSumOrderTotal = false);

        Task<ReOrderDto> PrepareReOrderDtoAsync(ReOrderServiceResults serviceResults);
    }
}
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Networks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public interface IGatewayClient
    {
        public string GatewayName { get;   }
        public string ErrorCallBackUrl { get;   }
        public string SuccessCallBackUrl { get;   }
        Task<HttpResponse<CreateInvoiceResponse>> CreateInvoiceAsync(IInvoiceRequest request);
        Task<HttpResponse<InvoiceStatusResponse>> GetInvoiceStatusAsync(GetInvoiceStatusRequest request);
    }
}
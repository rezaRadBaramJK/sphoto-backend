using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.Networks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public interface IGatewayService
    {
        /// <exception cref="BadRequestBusinessException"></exception>
        Task<HttpResponse<CreateInvoiceResponse>> SendInvoiceAsync(IGatewayClient gatewayClient,
            GatewayPaymentTranslation translation, InvoiceRequest invoiceRequest);
    }
}
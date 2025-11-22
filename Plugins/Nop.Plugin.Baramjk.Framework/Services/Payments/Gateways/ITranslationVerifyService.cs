using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public interface ITranslationVerifyService
    {
        public Task<VerifyResult> VerifyAsync(IGatewayClient gatewayClient, GetInvoiceStatusRequest request);
    }
}
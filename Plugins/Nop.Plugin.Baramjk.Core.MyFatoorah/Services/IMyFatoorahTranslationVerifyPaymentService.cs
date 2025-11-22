using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Services
{
    public interface IMyFatoorahTranslationVerifyPaymentService
    {
        Task<VerifyResult> VerifyByPaymentIdAsync(string paymentId);
        Task<VerifyResult> VerifyByInvoiceIdAsync(int invoiceId);

        Task<VerifyResult> VerifyAsync();

    }
}
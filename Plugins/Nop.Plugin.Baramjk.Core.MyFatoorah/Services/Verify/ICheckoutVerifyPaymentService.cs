using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services.Verify.Models;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Services.Verify
{
    public interface ICheckoutVerifyPaymentService
    {
        Task<CheckoutVerifyPaymentResponse> VerifyAsync(GetPaymentStatusRequest paymentStatusRequest);
    }
}
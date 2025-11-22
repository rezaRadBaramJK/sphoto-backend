using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.ExecutePayment;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.SendPayment;
using Nop.Plugin.Baramjk.Framework.Services.Networks;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Clients
{
    public interface IMyFatoorahPaymentClient
    {
        Task<HttpResponse<MyFatoorahResponse<SendPaymentDataResponse>>> SendPaymentAsync(SendPaymentRequest request);

        Task<MyFatoorahResponse<ExecutePaymentDataResponse>> ExecutePaymentAsync(ExecutePaymentRequest request);

        Task<MyFatoorahResponse<GetPaymentStatusResponse>> GetPaymentStatusAsync(GetPaymentStatusRequest request);
    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients.Models.Payments.GetPaymentStatus;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services.Verify;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Api
{
    public class CheckoutVerifyPaymentController : BaseBaramjkApiController
    {
        private readonly ICheckoutVerifyPaymentService _checkoutVerifyPaymentService;

        public CheckoutVerifyPaymentController(ICheckoutVerifyPaymentService checkoutVerifyPaymentService)
        {
            _checkoutVerifyPaymentService = checkoutVerifyPaymentService;
        }

        [HttpPost("/api-frontend/Checkout/myFatoorah/payment/verifyByInvoiceId")]
        [HttpPost("/FrontendApi/Checkout/myFatoorah/payment/verifyByInvoiceId")]
        public async Task<IActionResult> VerifyByInvoiceId([FromQuery] string invoiceId)
        {
            var paymentStatusRequest = GetPaymentStatusRequest.ByInvoiceId(invoiceId);
            return await VerifyAsync(paymentStatusRequest);
        }

        [HttpPost("/api-frontend/Checkout/myFatoorah/payment/verifyPaymentId")]
        [HttpPost("/FrontendApi/Checkout/myFatoorah/payment/verifyPaymentId")]
        public async Task<IActionResult> VerifyPaymentId([FromQuery] string paymentId)
        {
            var paymentStatusRequest = GetPaymentStatusRequest.ByPaymentId(paymentId);
            return await VerifyAsync(paymentStatusRequest);
        }

        private async Task<IActionResult> VerifyAsync(GetPaymentStatusRequest paymentStatusRequest)
        {
            var response = await _checkoutVerifyPaymentService.VerifyAsync(paymentStatusRequest);
            return ApiResponseFactory.Success(response);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Services.Payments;
#pragma warning disable CS1998

namespace Nop.Plugin.Baramjk.Framework.Nop.Plugins
{
    public abstract class BaramjkPaymentMethodPlugin : BaramjkPlugin, IPaymentMethod
    {
        public abstract string GetPublicViewComponentName();
        public abstract Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request);
        public abstract Task<string> GetPaymentMethodDescriptionAsync();

        public virtual async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
        }

        public virtual async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart) => false;

        public virtual async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart) => 0;

        public virtual async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
            => new() { Errors = new[] { "Capture method not supported" } };

        public virtual async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
            => new() { Errors = new[] { "Refund method not supported" } };

        public virtual async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest) =>
            new() { Errors = new[] { "Void method not supported" } };

        public virtual async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(
            ProcessPaymentRequest request) => new() { Errors = new[] { "Recurring payment not supported" } };

        public virtual async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(
            CancelRecurringPaymentRequest request) => new() { Errors = new[] { "Recurring payment not supported" } };

        public virtual async Task<bool> CanRePostProcessPaymentAsync(Order order)   {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            return true;
        }
        public virtual async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form) => new List<string>();
        public virtual async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form) => new();

        public virtual bool SupportCapture => false;
        public virtual bool SupportPartiallyRefund => false;
        public virtual bool SupportRefund => false;
        public virtual bool SupportVoid => false;
        public virtual RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;
        public virtual PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;
        public virtual bool SkipPaymentInfo => false;
    }
}
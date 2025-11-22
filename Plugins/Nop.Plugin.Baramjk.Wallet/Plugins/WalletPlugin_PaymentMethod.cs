using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Payments;
using Nop.Core.Infrastructure;
using Nop.Services.Payments;

namespace Nop.Plugin.Baramjk.Wallet.Plugins
{
    public partial class WalletPlugin  
    {
        public override async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var orderOrderTotal = processPaymentRequest.OrderTotal;

            var result = await _walletProcessPaymentService.PayAsync(orderOrderTotal);
            if (result != null)
                return new ProcessPaymentResult
                {
                    NewPaymentStatus = PaymentStatus.Pending,
                    Errors = new List<string> { result }
                };

            var processPaymentResult = new ProcessPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Paid
            };

            return processPaymentResult;
        }

        public override string GetPublicViewComponentName()
        {
            var settings = EngineContext.Current.Resolve<WalletSettings>();
            return settings.PublicViewComponentName;
        }
        public override Task<string> GetPaymentMethodDescriptionAsync() => Task.FromResult("Wallet Payment");

        public override bool SkipPaymentInfo
        {
            get
            {
                var settings = EngineContext.Current.Resolve<WalletSettings>();
                return settings.SkipPaymentInfo;
            }
        }
    }
}
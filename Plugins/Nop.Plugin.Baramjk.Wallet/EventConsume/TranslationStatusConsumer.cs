using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Events;

namespace Nop.Plugin.Baramjk.Wallet.EventConsume
{
    public class TranslationStatusConsumerConsumer : IConsumer<GatewayPaymentTranslationStatusEvent>
    {
        private readonly WalletChargeService _walletChargeService;

        public TranslationStatusConsumerConsumer(WalletChargeService walletChargeService)
        {
            _walletChargeService = walletChargeService;
        }


        public async Task HandleEventAsync(GatewayPaymentTranslationStatusEvent eventMessage)
        {
            var translation = eventMessage.Entity;
            if (translation.ConsumerName != DefaultValue.ConsumerName)
                return;
            if (eventMessage.JustPaid == false)
                return;
            
            await _walletChargeService.HandleTranslationStatusEventAsync(eventMessage);
        }
    }
}
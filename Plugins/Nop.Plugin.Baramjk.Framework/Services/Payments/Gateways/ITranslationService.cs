using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public interface ITranslationService
    {
        Task<GatewayPaymentTranslation> InsertAsync(GatewayPaymentTranslation translation);
        Task<GatewayPaymentTranslation> UpdateAsync(GatewayPaymentTranslation translation);
        Task<GatewayPaymentTranslation> GetByIdAsync(int id);
        Task<GatewayPaymentTranslation> GetByPaymentIdAsync(string paymentId);
        Task<GatewayPaymentTranslation> GetByGuidAsync(string guid);

        Task<List<GatewayPaymentTranslation>> GetConsumerTranslationsAsync
            (string consumerName = null, string consumerEntityType = null, int? consumerEntityId = null);

        Task<GatewayPaymentTranslation> SetConsumerStatusAsync(string guid, ConsumerStatus status);
        Task<GatewayPaymentTranslation> NewTranslationAsync(CreateTranslationRequest request);

        Task<GatewayPaymentTranslation> SetConsumerStatusAsync(GatewayPaymentTranslation translation,
            ConsumerStatus status);
    }
}
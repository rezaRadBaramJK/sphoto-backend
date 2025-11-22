using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Events.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways
{
    public class TranslationService : ITranslationService
    {
        private readonly IRepository<GatewayPaymentTranslation> _repositoryTranslation;
        private readonly IEventPublisher _eventPublisher;

        public TranslationService(IRepository<GatewayPaymentTranslation> repositoryTranslation,
            IEventPublisher eventPublisher)
        {
            _repositoryTranslation = repositoryTranslation;
            _eventPublisher = eventPublisher;
        }

        public async Task<GatewayPaymentTranslation> InsertAsync(GatewayPaymentTranslation translation)
        {
            await _repositoryTranslation.InsertAsync(translation);
            return translation;
        }

        public async Task<GatewayPaymentTranslation> UpdateAsync(GatewayPaymentTranslation translation)
        {
            await _repositoryTranslation.UpdateAsync(translation);
            return translation;
        }

        public async Task<GatewayPaymentTranslation> GetByIdAsync(int id)
        {
            return await _repositoryTranslation.Table.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<GatewayPaymentTranslation> GetByPaymentIdAsync(string paymentId)
        {
            return await _repositoryTranslation.Table
                .FirstOrDefaultAsync(item => item.PaymentId == paymentId);
        }

        public async Task<GatewayPaymentTranslation> GetByGuidAsync(string guid)
        {
            return await _repositoryTranslation.Table.FirstOrDefaultAsync(item => item.Guid == guid);
        }

        public async Task<List<GatewayPaymentTranslation>> GetConsumerTranslationsAsync
            (string consumerName = null, string consumerEntityType = null, int? consumerEntityId = null)
        {
            var translation = await _repositoryTranslation.Table
                .Where(item => (consumerName == null || item.ConsumerName == consumerName) &&
                               (consumerEntityType == null || item.ConsumerEntityType == consumerEntityType) &&
                               (consumerEntityId == null || item.ConsumerEntityId == consumerEntityId))
                .OrderByDescending(item => item.Id)
                .ToListAsync();

            return translation;
        }

        public async Task<GatewayPaymentTranslation> SetConsumerStatusAsync(string guid, ConsumerStatus status)
        {
            var translation = await GetByGuidAsync(guid);
            if (translation == null)
                throw NotFoundBusinessException.NotFound<GatewayPaymentTranslation>();

            return await SetConsumerStatusAsync(translation, status);
        }

        public async Task<GatewayPaymentTranslation> SetConsumerStatusAsync(GatewayPaymentTranslation translation,
            ConsumerStatus status)
        {
            if (translation == null)
                throw NotFoundBusinessException.NotFound<GatewayPaymentTranslation>();

            var statusEvent = new GatewayPaymentTranslationConsumerStatusEvent(translation, translation.ConsumerStatus);

            translation.ConsumerStatus = status;
            await _repositoryTranslation.UpdateAsync(translation);

            await _eventPublisher.PublishAsync(statusEvent);

            return translation;
        }

        public async Task<GatewayPaymentTranslation> NewTranslationAsync(CreateTranslationRequest request)
        {
            var guild = Guid.NewGuid().ToString();

            var translation = new GatewayPaymentTranslation
            {
                Guid = guild,
                AmountToPay = request.AmountToPay,
                GatewayName = request.GatewayName,
                PaymentOptionId = request.PaymentOptionId,
                OwnerCustomerId = request.CustomerId,
                ConsumerName = request.ConsumerName,
                ConsumerEntityType = request.ConsumerEntityType,
                ConsumerEntityId = request.ConsumerEntityId,
                ConsumerData = request.ConsumerData,
                ConsumerCallBackUrl = request.ConsumerCallBack,
                Status = GatewayPaymentStatus.Pending,
                ConsumerStatus = ConsumerStatus.Pending,
                OnCreateDateTimeUtc = DateTime.UtcNow,
                PaymentFeeRuleId = request.PaymentFeeRuleId,
                PaymentFeeValue = request.PaymentFeeValue,
                PaymentUrl = "",
                Message = "",
                PaymentId = ""
            };

            await InsertAsync(translation);
            return translation;
        }
    }
}
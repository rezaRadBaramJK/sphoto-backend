using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Services.Messages;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services
{
    public class OtpMessageTemplateService
    {
        private const string MessageTemplateName = "OtpAuthentication.VerificationEmail";

        private readonly IMessageTemplateService _messageTemplateService;

        public OtpMessageTemplateService(IMessageTemplateService messageTemplateService)
        {
            _messageTemplateService = messageTemplateService;
        }

        public async Task AddOtpMessageTemplateIfNotExistAsync()
        {
            var existTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(MessageTemplateName);
            if (existTemplates.Any())
                return;

            var messageTemplate = new MessageTemplate
            {
                Name = MessageTemplateName,
                Subject = "%Store.Name%. Verification code",
                Body = "Your verification code for %Store.Name%: {otp}<br /><br /><strong>%Store.Name%</strong>",
                IsActive = true,
            };
            await _messageTemplateService.InsertMessageTemplateAsync(messageTemplate);
        }

        public async Task DeleteOtpMessageTemplateAsync()
        {
            var existTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(MessageTemplateName);
            if (existTemplates.Any())
                return;
            foreach (var messageTemplate in existTemplates)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);
            }
        }

        public async Task<MessageTemplate> GetOtpMessageTemplateAsync()
        {
            var messages = await _messageTemplateService.GetMessageTemplatesByNameAsync(MessageTemplateName);
            return messages.LastOrDefault();
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Services.Messages;

namespace Nop.Plugin.Baramjk.ContactUs.Services
{
    public class ContactUsMessageTemplateService
    {
        private const string MessageTemplateName = "ContactUs.ContactOwnerPaymentSuccessful";
        
        private readonly IMessageTemplateService _messageTemplateService;

        public ContactUsMessageTemplateService(IMessageTemplateService messageTemplateService)
        {
            _messageTemplateService = messageTemplateService;
        }
        
        
        public async Task AddContactOwnerPaymentSuccessfulMessageTemplateIfNotExistAsync()
        {
            var existTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(MessageTemplateName);
            if (existTemplates.Any())
                return;

            var messageTemplate = new MessageTemplate
            {
                Name = MessageTemplateName,
                Subject = "%Store.Name%. Contact owner payment successful",
                Body = "{customerInfo}<br /><br /><strong>%Store.Name%</strong>",
                IsActive = true,
            };
            await _messageTemplateService.InsertMessageTemplateAsync(messageTemplate);
        }
        
        public async Task DeleteContactOwnerPaymentSuccessfulMessageTemplateAsync()
        {
            var existTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(MessageTemplateName);
            if (existTemplates.Any())
                return;
            foreach (var messageTemplate in existTemplates)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);
            }
        }
        
        public async Task<MessageTemplate> GetContactOwnerPaymentSuccessfulMessageTemplateAsync()
        {
            var messages = await _messageTemplateService.GetMessageTemplatesByNameAsync(MessageTemplateName);
            return messages.LastOrDefault();
        }
    }
}
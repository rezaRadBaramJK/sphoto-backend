using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Data;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class FrontendApiMessageTemplateService
    {
        private readonly IRepository<MessageTemplate> _messageTemplateRepository;

        public FrontendApiMessageTemplateService(IRepository<MessageTemplate> messageTemplateRepository)
        {
            _messageTemplateRepository = messageTemplateRepository;
        }

        public Task<List<MessageTemplate>> GetCustomerNewPasswordRecoveryMessagesAsync()
        {
            return _messageTemplateRepository.Table.Where(mt =>
                mt.Name == WebApiFrontendDefaults.CustomerGenerateNewPasswordRecoveryMessage)
                .ToListAsync();
        }


        public Task InitTemplatesAsync()
        {
            return AddCustomerGenerateNewPasswordRecoveryMessageTemplateAsync();
        }

        public async Task DeleteCustomerNewPasswordRecoveryMessageTemplateAsync()
        {
            var message = await GetCustomerNewPasswordRecoveryMessagesAsync();
            if(message == null)
                return;
            
            await _messageTemplateRepository.DeleteAsync(message);
        }

        private async Task AddCustomerGenerateNewPasswordRecoveryMessageTemplateAsync()
        {
            var isExists = await _messageTemplateRepository.Table
                .AnyAsync(mt => mt.Name == WebApiFrontendDefaults.CustomerGenerateNewPasswordRecoveryMessage);
            
            if(isExists)
                return;
            
            await _messageTemplateRepository.InsertAsync(new MessageTemplate
            {
                Name = WebApiFrontendDefaults.CustomerGenerateNewPasswordRecoveryMessage,
                IsActive = true,
                Subject = "%Store.Name%. Password recovery",
                Body = "<a href=\"%Store.URL%\">%Store.Name%</a>\n<br />\n<br />\nYour new Password Is: <b>%PasswordRecoveryToken%<b />\n\n<br />\n<br />\n%Store.Name%\n",
            });

        }
        
    }
}
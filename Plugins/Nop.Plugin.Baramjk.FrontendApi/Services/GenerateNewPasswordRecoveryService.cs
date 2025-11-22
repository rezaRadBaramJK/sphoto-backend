using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Events;
using Nop.Plugin.Baramjk.FrontendApi.Models.PasswordRecoveries;
using Nop.Plugin.Baramjk.FrontendApi.Models.Types;
using Nop.Plugin.Baramjk.FrontendApi.Services.Abstractions;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class GenerateNewPasswordRecoveryService : IPasswordRecoveryService
    {
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IEventPublisher _eventPublisher;
        private readonly FrontendApiMessageTemplateService _frontendApiMessageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        

        public GenerateNewPasswordRecoveryService(
            ICustomerService customerService,
            ILocalizationService localizationService, 
            IStoreContext storeContext,
            IWorkContext workContext,
            ILanguageService languageService,
            IMessageTokenProvider messageTokenProvider, 
            IEventPublisher eventPublisher,
            FrontendApiMessageTemplateService frontendApiMessageTemplateService,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            IWorkflowMessageService workflowMessageService,
            IGenericAttributeService genericAttributeService, 
            ILogger logger, 
            ICustomerRegistrationService customerRegistrationService, 
            CustomerSettings customerSettings)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
            _languageService = languageService;
            _messageTokenProvider = messageTokenProvider;
            _eventPublisher = eventPublisher;
            _frontendApiMessageTemplateService = frontendApiMessageTemplateService;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _workflowMessageService = workflowMessageService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _customerRegistrationService = customerRegistrationService;
            _customerSettings = customerSettings;
        }

        public PasswordRecoveryStrategy PasswordRecoveryStrategy => PasswordRecoveryStrategy.GenerateNewPassword;
        
        
        
        public async Task<PasswordRecoverySendResult> Send(PasswordRecoveryModel passwordRecoveryModel)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(passwordRecoveryModel.Email.Trim());
            
            var isSuccess = false;
            if (customer is { Active: true, Deleted: false })
            {
                var randomPassword = new Random().Next(100000, 999999);
                var changePassRequest = new ChangePasswordRequest(
                    customer.Email,
                    false, 
                    _customerSettings.DefaultPasswordFormat, 
                    randomPassword.ToString());

                var result = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
                if (result.Success == false)
                {
                    return new PasswordRecoverySendResult
                    {
                        PasswordRecoveryModel = new PasswordRecoveryModel
                        {
                            Email = customer.Email,
                            Result = string.Join(",", result.Errors)
                        }
                    };
                }
                
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                    randomPassword.ToString());
                //send email
                await SendCustomerNewPasswordRecoveryMessageAsync(customer);

                passwordRecoveryModel.Result =
                    await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent");
                isSuccess = true;
            }
            
            else
            {
                passwordRecoveryModel.Result =
                    await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound");
            }

            return new PasswordRecoverySendResult
            {
                Success = isSuccess,
                PasswordRecoveryModel = passwordRecoveryModel
            };
        }
        
        private async Task SendCustomerNewPasswordRecoveryMessageAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            var language = await _workContext.GetWorkingLanguageAsync();
            var languageId = await EnsureLanguageIsActiveAsync(language.Id, store.Id);

            var messageTemplates =
                await _frontendApiMessageTemplateService.GetCustomerNewPasswordRecoveryMessagesAsync();
            if (messageTemplates.Any() == false)
            {
                await _logger.ErrorAsync("Generate new password recovery message template not found.");
                return;
            }
                
            
            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }
        
        private async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language is not { Published: true })
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            
            if (language is not { Published: true })
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            
            if (language == null)
                throw new Exception("No active language could be loaded");

            return language.Id;
        }
        
        protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            return emailAccount;
        }
    }
}
using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.FrontendApi.Models.PasswordRecoveries;
using Nop.Plugin.Baramjk.FrontendApi.Models.Types;
using Nop.Plugin.Baramjk.FrontendApi.Services.Abstractions;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Factories;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class SendLinkPasswordRecoveryService : IPasswordRecoveryService
    {
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerModelFactory _customerModelFactory;

        public SendLinkPasswordRecoveryService(
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IWorkflowMessageService workflowMessageService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICustomerModelFactory customerModelFactory)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _localizationService = localizationService;
            _customerModelFactory = customerModelFactory;
        }

        public PasswordRecoveryStrategy PasswordRecoveryStrategy => PasswordRecoveryStrategy.SendLink;

        public async Task<PasswordRecoverySendResult> Send(PasswordRecoveryModel passwordRecoveryModel)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(passwordRecoveryModel.Email.Trim());
            var isSuccess = false;
            if (customer != null && customer.Active && !customer.Deleted)
            {
                //save token and current date
                var passwordRecoveryToken = new Random().Next(12345, 99999);
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                    passwordRecoveryToken.ToString());
                DateTime? generatedDateTime = DateTime.UtcNow;
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                //send email
                await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                    (await _workContext.GetWorkingLanguageAsync()).Id);

                passwordRecoveryModel.Result =
                    await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent");
                isSuccess = true;
            }
            else
            {
                passwordRecoveryModel.Result =
                    await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound");
            }

            passwordRecoveryModel =
                await _customerModelFactory.PreparePasswordRecoveryModelAsync(passwordRecoveryModel);

            return new PasswordRecoverySendResult
            {
                Success = isSuccess,
                PasswordRecoveryModel = passwordRecoveryModel,
            };
        }
    }
}
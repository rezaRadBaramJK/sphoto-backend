using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class CustomerRegistrationExService : CustomerRegistrationService
    {
        public CustomerRegistrationExService(CustomerSettings customerSettings,
            IAuthenticationService authenticationService, ICustomerActivityService customerActivityService,
            ICustomerService customerService, IEncryptionService encryptionService, IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService, ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService, INotificationService notificationService,
            IRewardPointService rewardPointService, IShoppingCartService shoppingCartService,
            IStoreContext storeContext, IStoreService storeService, IWorkContext workContext,
            IWorkflowMessageService workflowMessageService, RewardPointsSettings rewardPointsSettings) : base(
            customerSettings, authenticationService, customerActivityService, customerService, encryptionService,
            eventPublisher, genericAttributeService, localizationService, multiFactorAuthenticationPluginManager,
            newsLetterSubscriptionService, notificationService, rewardPointService, shoppingCartService, storeContext,
            storeService, workContext, workflowMessageService, rewardPointsSettings)
        {
        }

        public override async Task<CustomerRegistrationResult> RegisterCustomerAsync(
            CustomerRegistrationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Username) == false)
            {
                if (CommonHelper.IsValidEmail(request.Username))
                    request.Email = request.Username;

                var next = new Random().Next(1111, 9999);
                request.Email = request.Username.Replace("@", "") + next + "@xyz.com";
            }

            return await base.RegisterCustomerAsync(request);
        }
    }
}
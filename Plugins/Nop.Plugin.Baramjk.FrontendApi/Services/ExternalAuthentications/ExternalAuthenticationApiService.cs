using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.Baramjk.FrontendApi.Exceptions.Auth;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.OAuthTokenParsers;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications
{
    public class ExternalAuthenticationApiService
    {
        private readonly AppleOAuthTokenParser _appleOAuthTokenParser;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IAuthorizationUserService _authorizationUserService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<ExternalAuthenticationRecord> _externalAuthenticationRecordRepository;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly FaceBookOAuthTokenParser _faceBookOAuthTokenParser;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly GoogleOAuthTokenParser _googleOAuthTokenParser;
        private readonly ILocalizationService _localizationService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly PictureHelper _pictureHelper;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILogger _logger;
        public ExternalAuthenticationApiService(IAuthorizationUserService authorizationUserService,
            IWorkContext workContext, ICustomerService customerService, GoogleOAuthTokenParser googleOAuthTokenParser,
            AppleOAuthTokenParser appleOAuthTokenParser, FaceBookOAuthTokenParser faceBookOAuthTokenParser,
            IGenericAttributeService genericAttributeService,
            IRepository<ExternalAuthenticationRecord> externalAuthenticationRecordRepository,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            IAuthenticationPluginManager authenticationPluginManager,
            ICustomerRegistrationService customerRegistrationService, IEventPublisher eventPublisher,
            ILocalizationService localizationService, IStoreContext storeContext,
            IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings,
            CustomerSettings customerSettings, ICustomerActivityService customerActivityService,
            PictureHelper pictureHelper, ILogger logger)
        {
            _authorizationUserService = authorizationUserService;
            _workContext = workContext;
            _customerService = customerService;
            _googleOAuthTokenParser = googleOAuthTokenParser;
            _appleOAuthTokenParser = appleOAuthTokenParser;
            _faceBookOAuthTokenParser = faceBookOAuthTokenParser;
            _genericAttributeService = genericAttributeService;
            _externalAuthenticationRecordRepository = externalAuthenticationRecordRepository;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _authenticationPluginManager = authenticationPluginManager;
            _customerRegistrationService = customerRegistrationService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _customerSettings = customerSettings;
            _customerActivityService = customerActivityService;
            _pictureHelper = pictureHelper;
            _logger = logger;
        }

        /// <exception cref="IncorrectInfoException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="EmailAlreadyExistsException"></exception>
        public async Task<AuthenticateResponse> SignInAsync(SignInModel model)
        {
            var authTokenDataResult = await GetOAuthTokenData(model);
            if (authTokenDataResult.IsSuccess == false)
                throw new IncorrectInfoException(
                    authTokenDataResult?.ValidateResult?.Exception?.Message ?? "Cant Get OAuth token data");

            var authenticationParameters = GetParametersAsync(authTokenDataResult.OAuthTokenData);
            //  return null;
            var customer =
                await GetOrCreateCustomerFromToken(authenticationParameters, authTokenDataResult.OAuthTokenData);

            await SignInCustomerAsync(customer);
            var response = await _authorizationUserService.AuthenticateAsync(customer);
            return response;
        }

        private async Task<OAuthTokenDataResult> GetOAuthTokenData(SignInModel model)
        {
            var authTokenDataResult = model.Provider switch
            {
                OAuthProvider.Google => await _googleOAuthTokenParser.GetOAuthTokenDataAsync(model),
                OAuthProvider.Apple => await _appleOAuthTokenParser.GetOAuthTokenDataAsync(model),
                OAuthProvider.Facebook => await _faceBookOAuthTokenParser.GetOAuthTokenDataAsync(model),
                _ => throw new ArgumentOutOfRangeException(nameof(model.Provider), model.Provider, null)
            };

            return authTokenDataResult;
        }

        private ExternalAuthenticationParameters GetParametersAsync(OAuthTokenData data)
        {
            var authenticationParameters = new ExternalAuthenticationParameters
            {
                ProviderSystemName = FrontendAuthenticationDefaults.SystemName,
                AccessToken = data.AccessToken,
                Email = data.Email,
                ExternalIdentifier = data.ExternalIdentifier,
                ExternalDisplayIdentifier = data.ExternalDisplayIdentifier,
                Claims = data.Claims
            };

            return authenticationParameters;
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="EmailAlreadyExistsException"></exception>
        /// <exception cref="Exception"></exception>
        protected async Task<Customer> GetOrCreateCustomerFromToken(ExternalAuthenticationParameters parameters,
            OAuthTokenData oAuthTokenData)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var associatedUser = await GetUserByExternalIdentifierAsync(parameters.ExternalIdentifier);
            if (associatedUser != null)
                return associatedUser;
            var currentCustomer = await _customerService.GetCustomerByEmailAsync(parameters.Email);
            var isRegistered = false;
            if (currentCustomer != null)
            {
                isRegistered = await _customerService.IsRegisteredAsync(currentCustomer);
            }
            // if (_customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
            // {
            //     throw new Exception(
            //         $"login with {parameters.ProviderSystemName} is only available when UserRegistrationType is EmailValidation.");
            // }
            
            if (isRegistered)
            {
                if (!currentCustomer.Active)
                {
                    throw new Exception(
                        $"login with {parameters.ProviderSystemName} is not available until email verification is completed.");
                }
                if (currentCustomer.Email == parameters.Email)
                {
                    await InsertExternalAuthenticationRecordAsync(currentCustomer, parameters);
                    return currentCustomer;
                }
            }
            
            var createCustomerFromToken = await RegisterNewUserAsync(parameters);
            await UpdateCustomerInfoAsync(oAuthTokenData, createCustomerFromToken);
            return createCustomerFromToken;
        }

        protected virtual async Task<Customer> GetUserByExternalIdentifierAsync(string externalIdentifier)
        {
            var associationRecord = _externalAuthenticationRecordRepository.Table
                .FirstOrDefault(record =>
                    record.ExternalIdentifier.Equals(externalIdentifier) &&
                    record.ProviderSystemName.Equals(FrontendAuthenticationDefaults.SystemName));

            if (associationRecord == null)
                return null;

            var customer = await _customerService.GetCustomerByIdAsync(associationRecord.CustomerId);
            if (customer == null || customer.Deleted)
            {
                return null;
            }

            return customer;
        }

        protected virtual async Task InsertExternalAuthenticationRecordAsync(Customer customer,
            ExternalAuthenticationParameters parameters)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var externalAuthenticationRecord = new ExternalAuthenticationRecord
            {
                CustomerId = customer.Id,
                Email = parameters.Email,
                ExternalIdentifier = parameters.ExternalIdentifier,
                ExternalDisplayIdentifier = parameters.ExternalDisplayIdentifier,
                OAuthAccessToken = parameters.AccessToken,
                ProviderSystemName = parameters.ProviderSystemName
            };

            await _externalAuthenticationRecordRepository.InsertAsync(externalAuthenticationRecord, false);
        }

        /// <exception cref="Exception"></exception>
        /// <exception cref="EmailAlreadyExistsException"></exception>
        protected virtual async Task<Customer> RegisterNewUserAsync(ExternalAuthenticationParameters parameters)
        {
            var canRegisterUser = _customerSettings.UserRegistrationType != UserRegistrationType.Disabled;
            if (canRegisterUser == false)
                throw new Exception("Registration is disabled");

            //check whether the specified email has been already registered
            if (await _customerService.GetCustomerByEmailAsync(parameters.Email) != null)
            {
                // var alreadyExistsError = string.Format(
                //     await _localizationService.GetResourceAsync("Account.AssociatedExternalAuth.EmailAlreadyExists"),
                //     !string.IsNullOrEmpty(parameters.ExternalDisplayIdentifier)
                //         ? parameters.ExternalDisplayIdentifier
                //         : parameters.ExternalIdentifier);
                var alreadyExistsError = "A user with the specified email has been already registered.";

                throw new EmailAlreadyExistsException(alreadyExistsError);
            }

            //registration is approved if validation isn't required
            var registrationIsApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard ||
                                         (_customerSettings.UserRegistrationType ==
                                          UserRegistrationType.EmailValidation &&
                                          !_externalAuthenticationSettings.RequireEmailValidation);

            var newCustomer = await _customerService.InsertGuestCustomerAsync();

            //create registration request
            var registrationRequest = new CustomerRegistrationRequest(newCustomer,
                parameters.Email, parameters.Email,
                CommonHelper.GenerateRandomDigitCode(20),
                PasswordFormat.Hashed,
                (await _storeContext.GetCurrentStoreAsync()).Id,
                registrationIsApproved);

            //whether registration request has been completed successfully
            var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
            if (!registrationResult.Success)
            {
                var error = string.Join(Environment.NewLine, registrationResult.Errors);
                throw new Exception(error);
            }

            //associate external account with registered user
            await InsertExternalAuthenticationRecordAsync(newCustomer, parameters);

            //authenticate
            if (registrationIsApproved)
                //raise event       
                await _eventPublisher.PublishAsync(new CustomerActivatedEvent(newCustomer));

            //allow to save other customer values by consuming this event
            await _eventPublisher.PublishAsync(
                new CustomerAutoRegisteredByExternalMethodEvent(newCustomer, parameters));
            //raise customer registered event
            await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(newCustomer));

            //store owner notifications
            if (_customerSettings.NotifyNewCustomerRegistration)
                await _workflowMessageService.SendCustomerRegisteredNotificationMessageAsync(newCustomer,
                    _localizationSettings.DefaultAdminLanguageId);

            return newCustomer;
        }

        protected async Task UpdateCustomerInfoAsync(OAuthTokenData oAuthTokenData, Customer customer)
        {
            if (string.IsNullOrEmpty(oAuthTokenData.Gender) == false)
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.GenderAttribute,
                    oAuthTokenData.Gender);

            if (string.IsNullOrEmpty(oAuthTokenData.FirstName) == false)
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute,
                    oAuthTokenData.FirstName);

            if (string.IsNullOrEmpty(oAuthTokenData.LastName) == false)
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute,
                    oAuthTokenData.LastName);

            if (oAuthTokenData.Birthdate != null)
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.DateOfBirthAttribute, oAuthTokenData.Birthdate);

            if (string.IsNullOrEmpty(oAuthTokenData.MobileNumber) == false)
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.PhoneAttribute, oAuthTokenData.MobileNumber);

            await UpdateAvatarAsync(oAuthTokenData, customer);
        }

        protected async Task UpdateAvatarAsync(OAuthTokenData oAuthTokenData, Customer customer)
        {
            try
            {
                var picture = await _pictureHelper.GetPictureFromUrlAsync(oAuthTokenData.Avatar, "avatar");
                if (picture == null)
                    return;

                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute, picture.Id);
            }
            catch (Exception)
            {
            }
        }

        protected virtual async Task SignInCustomerAsync(Customer customer)
        {
            await _workContext.SetCurrentCustomerAsync(customer);

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

            //activity log
            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);
        }
    }
}
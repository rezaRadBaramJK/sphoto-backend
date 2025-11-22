using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Baramjk.FrontendApi.Framework;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Services;
using Nop.Plugin.Baramjk.FrontendApi.Models;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class AuthorizationUserService : IAuthorizationUserService
    {
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IEncryptionService _encryptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INotificationService _notificationService;
        private readonly IRepository<GenericAttribute> _repositoryGenericAttribute;
        private readonly IStoreContext _storeContext;
        private readonly FrontendApiSettings _webApiCommonSettings;
        private readonly IWorkContext _workContext;


        public AuthorizationUserService(CustomerSettings customerSettings,
            ICustomerRegistrationService customerRegistrationService, ICustomerService customerService,
            IJwtTokenService jwtTokenService, IWorkContext workContext, IEncryptionService encryptionService,
            IGenericAttributeService genericAttributeService, ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INotificationService notificationService, IStoreContext storeContext,
            IRepository<GenericAttribute> repositoryGenericAttribute, FrontendApiSettings webApiCommonSettings,
            ILogger logger)
        {
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _jwtTokenService = jwtTokenService;
            _workContext = workContext;
            _encryptionService = encryptionService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _repositoryGenericAttribute = repositoryGenericAttribute;
            _webApiCommonSettings = webApiCommonSettings;
            _logger = logger;
        }


        public virtual async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateCustomerRequest request)
        {
            Customer customer;

            if (request.IsGuest)
            {
                customer = await _customerService.InsertGuestCustomerAsync();
                await _workContext.SetCurrentCustomerAsync(customer);

                return await GenerateAuthenticateResponse(customer);
            }

            customer = await GetCustomerAsync(request);
            var result = await ValidateCustomerAsync(customer, request, string.IsNullOrEmpty(request.RefreshToken));

            if (result.LoginResults == CustomerLoginResults.Successful)
            {
                _ = await _customerRegistrationService.SignInCustomerAsync(result.Customer, null);
                return await GenerateAuthenticateResponse(result.Customer);
            }
            
            return null;
        }

        public virtual async Task<AuthenticateResponse> AuthenticateAsync(Customer customer)
        {
            return await GenerateAuthenticateResponse(customer);
        }

        public virtual async Task<Customer> GetCustomerAsync(AuthenticateCustomerRequest request)
        {
            Customer customer = null;

            if (string.IsNullOrEmpty(request.PhoneNumber) == false)
            {
                var genericAttribute = _repositoryGenericAttribute.Table.FirstOrDefault(item =>
                    item.KeyGroup == "Customer" &&
                    item.Key == NopCustomerDefaults.PhoneAttribute &&
                    item.Value == request.PhoneNumber);
                if (genericAttribute == null)
                    return null;

                customer = await _customerService.GetCustomerByIdAsync(genericAttribute.EntityId);
            }
            else if (string.IsNullOrEmpty(request.Email) == false)
            {
                customer = await _customerService.GetCustomerByEmailAsync(request.Email);
            }
            else if (string.IsNullOrEmpty(request.Username) == false)
            {
                customer = await _customerService.GetCustomerByUsernameAsync(request.Username);
            }
            else if (string.IsNullOrEmpty(request.RefreshToken) == false)
            {
                customer = await GetCustomerByRefreshTokenAsync(request.RefreshToken);
            }

            return customer;
        }

        public virtual async Task<CustomerLoginResultsModel> ValidateCustomerAsync(Customer customer,
            AuthenticateCustomerRequest request, bool checkPasswordsMatch = true)
        {
            if (customer == null)
                return new CustomerLoginResultsModel(customer, CustomerLoginResults.CustomerNotExist);
            if (customer.Deleted)
                return new CustomerLoginResultsModel(customer, CustomerLoginResults.Deleted);
            if (!customer.Active)
                return new CustomerLoginResultsModel(customer, CustomerLoginResults.NotActive);
            //only registered can login
            if (!await _customerService.IsRegisteredAsync(customer) && await _customerService.IsGuestAsync(customer) == false)
                return new CustomerLoginResultsModel(customer, CustomerLoginResults.NotRegistered);
            //check whether a customer is locked out
            if (customer.CannotLoginUntilDateUtc.HasValue && customer.CannotLoginUntilDateUtc.Value > DateTime.UtcNow)
                return new CustomerLoginResultsModel(customer, CustomerLoginResults.LockedOut);

            if (checkPasswordsMatch && !PasswordsMatch(await _customerService.GetCurrentPasswordAsync(customer.Id),
                    request.Password))
            {
                //wrong password
                customer.FailedLoginAttempts++;
                if (_customerSettings.FailedPasswordAllowedAttempts > 0 &&
                    customer.FailedLoginAttempts >= _customerSettings.FailedPasswordAllowedAttempts)
                {
                    //lock out
                    customer.CannotLoginUntilDateUtc =
                        DateTime.UtcNow.AddMinutes(_customerSettings.FailedPasswordLockoutMinutes);
                    //reset the counter
                    customer.FailedLoginAttempts = 0;
                }

                await _customerService.UpdateCustomerAsync(customer);

                return new CustomerLoginResultsModel(customer, CustomerLoginResults.WrongPassword);
            }

            var selectedProvider = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute);
            var methodIsActive = await _multiFactorAuthenticationPluginManager.IsPluginActiveAsync(selectedProvider,
                customer, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (methodIsActive)
                return new CustomerLoginResultsModel(customer, CustomerLoginResults.MultiFactorAuthenticationRequired);
            if (!string.IsNullOrEmpty(selectedProvider))
                _notificationService.WarningNotification(
                    await _localizationService.GetResourceAsync(
                        "MultiFactorAuthentication.Notification.SelectedMethodIsNotActive"));

            //update login details
            customer.FailedLoginAttempts = 0;
            customer.CannotLoginUntilDateUtc = null;
            customer.RequireReLogin = false;
            customer.LastLoginDateUtc = DateTime.UtcNow;
            await _customerService.UpdateCustomerAsync(customer);

            return new CustomerLoginResultsModel(customer, CustomerLoginResults.Successful);
        }

        protected bool PasswordsMatch(CustomerPassword customerPassword, string enteredPassword)
        {
            if (customerPassword == null || string.IsNullOrEmpty(enteredPassword))
                return false;

            var savedPassword = string.Empty;
            switch (customerPassword.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    savedPassword = enteredPassword;
                    break;
                case PasswordFormat.Encrypted:
                    savedPassword = _encryptionService.EncryptText(enteredPassword);
                    break;
                case PasswordFormat.Hashed:
                    savedPassword = _encryptionService.CreatePasswordHash(enteredPassword,
                        customerPassword.PasswordSalt, _customerSettings.HashedPasswordFormat);
                    break;
            }

            if (customerPassword.Password == null)
                return false;

            return customerPassword.Password.Equals(savedPassword);
        }

        private async Task<AuthenticateResponse> GenerateAuthenticateResponse(Customer customer)
        {
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
            var roles = customerRoles.Select(item => new RoleItem
            {
                Id = item.Id,
                Name = item.Name,
                SystemName = item.SystemName
            }).ToList();

            var currentTime = DateTimeOffset.Now;
            var expire = currentTime.AddDays(_webApiCommonSettings.TokenLifetimeDays);
            var jwtToken = _jwtTokenService.GetNewJwtToken(customer);
            var refreshToken = GetRefreshToken(customer);

            return new AuthenticateResponse
            {
                CustomerId = customer.Id,
                Roles = roles,
                ExpireDate = expire,
                Token = jwtToken,
                RefreshToken = refreshToken,
                Username = _customerSettings.UsernamesEnabled ? customer.Username : customer.Email
            };
        }

        public virtual string GetRefreshToken(Customer customer)
        {
            var expiresInSeconds = DateTimeOffset.Now.AddDays(30).ToUnixTimeSeconds();

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Exp, expiresInSeconds.ToString()),
                new(WebApiCommonDefaults.ClaimTypeName, customer.Id.ToString()),
                new(ClaimTypes.NameIdentifier, customer.CustomerGuid.ToString())
            };

            return _jwtTokenService.GetJwtToken(claims);
        }

        private async Task<Customer> GetCustomerByRefreshTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_webApiCommonSettings.SecretKey);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var customerId =
                    int.Parse(jwtToken.Claims.First(x => x.Type == WebApiCommonDefaults.ClaimTypeName).Value);

                return await _customerService.GetCustomerByIdAsync(customerId);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.ToString());
                return null;
            }
        }


        public class CustomerLoginResultsModel
        {
            public CustomerLoginResultsModel(Customer customer, CustomerLoginResults loginResults)
            {
                LoginResults = loginResults;
                Customer = customer;
            }

            public CustomerLoginResults LoginResults { get; set; }
            public Customer Customer { get; set; }
        }
    }
}
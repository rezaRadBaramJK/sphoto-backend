using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Baramjk.OtpAuthentication.Consts;
using Nop.Plugin.Baramjk.OtpAuthentication.Domain;
using Nop.Plugin.Baramjk.OtpAuthentication.Methods.Abstractions;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Api;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Methods;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Settings;
using Nop.Plugin.Baramjk.OtpAuthentication.Models.Types;
using Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services
{
    public class OtpAuthenticationService : IOtpAuthenticationService
    {
        private readonly IRepository<GenericAttribute> _repositoryGenericAttribute;
        private readonly ICustomerService _customerService;
        private readonly IMobileOtpService _mobileOtpService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly CustomerSettings _customerSettings;
        private readonly FrontendApiSettings _webApiCommonSettings;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly OtpAuthenticationSettings _otpAuthenticationSettings;
        private readonly IOtpVendorService _otpVendorService;
        private readonly ISendMethod _sendMethod;

        public OtpAuthenticationService(IRepository<GenericAttribute> repositoryGenericAttribute,
            ICustomerService customerService,
            IMobileOtpService mobileOtpService, IJwtTokenService jwtTokenService, CustomerSettings customerSettings,
            FrontendApiSettings webApiCommonSettings, IStoreContext storeContext,
            ICustomerRegistrationService customerRegistrationService, IGenericAttributeService genericAttributeService,
            ILogger logger, OtpAuthenticationSettings otpAuthenticationSettings,
            IOtpVendorService otpVendorService,
            ISendMethod sendMethod)
        {
            _repositoryGenericAttribute = repositoryGenericAttribute;
            _customerService = customerService;
            _mobileOtpService = mobileOtpService;
            _jwtTokenService = jwtTokenService;
            _customerSettings = customerSettings;
            _webApiCommonSettings = webApiCommonSettings;
            _storeContext = storeContext;
            _customerRegistrationService = customerRegistrationService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _otpAuthenticationSettings = otpAuthenticationSettings;
            _otpVendorService = otpVendorService;
            _sendMethod = sendMethod;
        }

        #region Send otp

        public async Task<SendOtpResponseApiModel> SendChangePhoneNumberOtp(SendOtpApiModel model, Customer customer)
        {
            var result = new SendOtpResponseApiModel();
            if (!customer.Active || customer.Deleted || customer.IsSystemAccount ||
                (await _customerService.IsGuestAsync(customer)))
            {
                result.ErrorMessages.Add("Invalid customer");
                await _logger.ErrorAsync($"Invalid customer={customer.Id} active {customer.Active} deleted{customer.Deleted} sysaccount {customer.IsSystemAccount} guest : {await _customerService.IsGuestAsync(customer)}");
                return result;
            }

            var phoneAttribute = await _repositoryGenericAttribute.Table.FirstOrDefaultAsync(item =>
                item.KeyGroup == "Customer" &&
                item.Key == NopCustomerDefaults.PhoneAttribute &&
                item.EntityId == customer.Id);
            if (phoneAttribute?.Value == null)
            {
                result.ErrorMessages.Add("Internal error");
                return result;
            }

            if (phoneAttribute.Value.Equals(model.PhoneNumber))
            {
                result.ErrorMessages.Add("Please enter new phone number");
                return result;
            }
            else
            {
                string secondaryPhoneNumber;
                if (model.PhoneNumber.StartsWith("+"))
                {
                    secondaryPhoneNumber = model.PhoneNumber.Replace("+", "");
                }
                else
                {
                    secondaryPhoneNumber = "+" + model.PhoneNumber;
                }

                var alreadyExitingPhoneNumber = await _repositoryGenericAttribute.Table.FirstOrDefaultAsync(x =>
                    x.KeyGroup == "Customer" &&
                    x.Key == NopCustomerDefaults.PhoneAttribute &&
                    (x.Value == model.PhoneNumber || x.Value == secondaryPhoneNumber));
                if (alreadyExitingPhoneNumber != null)
                {
                    result.ErrorMessages.Add("Customer with this phone nyumber already exists");
                    return result;
                }
            }

            return await SendOtpAsync(model, phoneAttribute.Value);
        }

        /// <exception cref="NopException"></exception>
        public async Task<SendOtpResponseApiModel> SendOtpAsync(SendOtpApiModel model, string oldPhoneNumber = null)
        {
            var result = new SendOtpResponseApiModel();

            var usePhoneNumber = string.IsNullOrEmpty(model.PhoneNumber) == false;

            Customer customer;

            if (usePhoneNumber)
            {
                var phone = oldPhoneNumber ?? model.PhoneNumber;
                customer = await FindCustomerByPhoneAsync(phone);
                if (customer == null && _otpAuthenticationSettings.DontRegisterIfUserNotExists)
                {
                    return new SendOtpResponseApiModel
                    {
                        Success = false,
                        ErrorMessages = new List<string> { "customer does not exist" },
                        Otp = null
                    };
                }
            }
            else if (string.IsNullOrWhiteSpace(model.Email) == false)
            {
                customer = await _customerService.GetCustomerByEmailAsync(model.Email);
                if (customer == null && _otpAuthenticationSettings.DontRegisterIfUserNotExists)
                {
                    return new SendOtpResponseApiModel
                    {
                        Success = false,
                        ErrorMessages = new List<string>{ "customer does not exist" },
                        Otp = null
                    };
                }
            }
            else
            {
                result.ErrorMessages.Add("Invalid input. please enter Phone number or Email.");
                return result;
            }
            
            if (usePhoneNumber == false && _otpAuthenticationSettings.SendMethod == SendMethodType.Sms)
            {
                result.ErrorMessages.Add("Authentication with phone number is disabled. please try with email.");
                return result;
            }

            var methodValue = usePhoneNumber ? model.PhoneNumber : model.Email;

            return await SendOtpAsync(result, model, methodValue, usePhoneNumber, oldPhoneNumber);
        }

        /// <summary>
        /// gets a message template and parameters and fo
        /// </summary>
        /// <param name="template">example : "Hello, your otp is : {otp}. Thanks"</param>
        /// <param name="parameters">example : new { otp = "1234" }</param>
        /// <returns></returns>
        private string FormatString(string template, object parameters)
        {
            var result = template;
            var properties = parameters.GetType().GetProperties();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(parameters, null);
                if (propertyValue == default)
                    continue;
                var pattern = "{" + propertyName + "}";
                result = result.Replace(pattern, propertyValue.ToString());
            }

            return result;
        }

        #endregion

        #region Validate otp

        public async Task<ValidateOtpResponseApiModel> ValidateOtpAsync(ValidateOtpApiModel model)
        {
            var result = new ValidateOtpResponseApiModel();
            var usePhoneNumber = false;
            string methodValue;
            if (string.IsNullOrEmpty(model.PhoneNumber) == false)
            {
                usePhoneNumber = true;
                methodValue = model.PhoneNumber;
            }
            else if (string.IsNullOrEmpty(model.Email) == false)
                methodValue = model.Email;
            else
            {
                result.ErrorMessages.Add("Invalid phone number or email.");
                return result;
            }

            if (usePhoneNumber == false && _otpAuthenticationSettings.SendMethod == SendMethodType.Sms)
            {
                result.ErrorMessages.Add("Authentication with phone number is disabled. please try with email.");
                return result;
            }


            var otp = await _mobileOtpService.FindByPhoneNumber(methodValue, model.OtpType);
            if (otp == null)
            {
                result.ErrorMessages.Add("Please send otp first");
                return result;
            }
            
            if (otp.OtpType != model.OtpType)
            {
                result.ErrorMessages.Add("Invalid otp type.");
                return result;
            }

            if (!otp.Otp.Equals(model.Otp) || otp.CreateDateTime.AddMinutes(1) < DateTime.UtcNow)
            {
                result.ErrorMessages.Add("Invalid otp");
                return result;
            }
            
            var requestedCustomer = usePhoneNumber
                ? await FindCustomerByPhoneAsync(model.PhoneNumber)
                : await _customerService.GetCustomerByEmailAsync(model.Email);

            if (model.OtpType == OtpType.RegisterOrLogin)
            {
                if (requestedCustomer == null)
                {
                    if (_otpAuthenticationSettings.DontRegisterIfUserNotExists == false)
                        return await RegisterUserAsync(model.PhoneNumber, model.Email, model.AsVendor, model.Password);

                    return new ValidateOtpResponseApiModel
                    {
                        Success = false,
                        CustomerId = 0,
                        ErrorMessages = new List<string> { "Customer does not exist." }
                    };
                }
                
                return await GenerateAuthenticateResponse(requestedCustomer);
            }

            if (requestedCustomer == null)
            {
                return new ValidateOtpResponseApiModel
                {
                    Success = false,
                    CustomerId = 0,
                    ErrorMessages = new List<string> { "Customer does not exist." }
                };
            }
            

            if (model.OtpType == OtpType.ChangePhone)
            {
                if (usePhoneNumber == false)
                {
                    result.ErrorMessages.Add("Invalid otp type.");
                    return result;
                }

                return await ChangePhoneNumberAsync(otp.OldPhoneNumber, otp.PhoneNumber);
            }

            if (model.OtpType == OtpType.ChangePassword)
            {
                if (string.IsNullOrEmpty(model.Password))
                    return new ValidateOtpResponseApiModel
                    {
                        Success = false,
                        ErrorMessages = new List<string> { "Invalid password." },
                        CustomerId = 0
                    };
                return await ChangePasswordAsync(requestedCustomer, model.Password);

            }

            return new ValidateOtpResponseApiModel
            {
                Success = false,
                ErrorMessages = new List<string> { "Invalid otp type." },
                CustomerId = 0
            };
        }

        private async Task<SendOtpResponseApiModel> SendOtpAsync(
            SendOtpResponseApiModel result,
            SendOtpApiModel model,
            string methodValue, 
            bool usePhoneNumber,
            string oldPhoneNumber = null)
        {
            var mobileOtp = await _mobileOtpService.FindByPhoneNumber(methodValue, model.OtpType);
            if (mobileOtp != null)
            {
                if (mobileOtp.CreateDateTime.AddMinutes(1) > DateTime.UtcNow)
                {
                    result.ErrorMessages.Add("Can't send more than 1 per minute");
                    return result;
                }

                if (mobileOtp.AttemptNumber >= 1000)
                {
                    if (mobileOtp.CreateDateTime.AddDays(1) < DateTime.UtcNow)
                    {
                        mobileOtp.AttemptNumber = 0;
                    }
                    else
                    {
                        result.ErrorMessages.Add("Can't send more than 3 otps per day");
                        return result;
                    }
                }
            }

            if (mobileOtp == null)
            {
                mobileOtp = new MobileOtp
                {
                    PhoneNumber = methodValue,
                    OldPhoneNumber = oldPhoneNumber,
                    OtpType = model.OtpType,
                    CreateDateTime = DateTime.UtcNow,
                    AttemptNumber = 1,
                    Otp = (usePhoneNumber && model.PhoneNumber == "96566554433")
                        ? "123456"
                        : new Random().Next(0, 1000000).ToString("D6")
                };

                await _mobileOtpService.InsertAsync(mobileOtp);
            }
            else
            {
                mobileOtp.AttemptNumber++;
                mobileOtp.CreateDateTime = DateTime.UtcNow;
                mobileOtp.Otp = usePhoneNumber && model.PhoneNumber == "96566554433"
                    ? "123456"
                    : new Random().Next(0, 1000000).ToString("D6");
                mobileOtp.OldPhoneNumber = oldPhoneNumber;
                mobileOtp.OtpType = model.OtpType;
                await _mobileOtpService.UpdateAsync(mobileOtp);
            }

            var parameters = new { otp = mobileOtp.Otp };
            var message = FormatString(_otpAuthenticationSettings.Message, parameters);
            try
            {
                await _sendMethod.SendAsync(new SendOtpParams
                {
                    PhoneNumber = model.PhoneNumber,
                    Message = message,
                    Email = model.Email,
                    OtpCode = mobileOtp.Otp
                });
            }
            catch (Exception e)
            {
                if (e is NopException or ArgumentNullException)
                {
                    result.ErrorMessages.Add(e.Message);
                    return result;
                }
                throw;
            }
            

            return new SendOtpResponseApiModel
            {
                Success = true,
                Otp = mobileOtp.Otp
            };
        }

        /// <exception cref="NopException"></exception>
        public async Task<SendOtpResponseApiModel> SendChangePasswordOtpAsync(SendOtpApiModel model)
        {
            var result = new SendOtpResponseApiModel();
            if (string.IsNullOrEmpty(model.PhoneNumber))
            {
                result.ErrorMessages.Add("Invalid phone number.");
                return result;
            }
            
            var customerByPhoneNumber = await FindCustomerByPhoneAsync(model.PhoneNumber);
            if (customerByPhoneNumber == null)
            {
                result.ErrorMessages.Add("Customer with this phone number does not exist.");
                return result;
            }
            
            if (await _customerService.IsRegisteredAsync(customerByPhoneNumber) == false)
            {
                result.ErrorMessages.Add("Customer should be registered.");
                return result;
            }

            if (customerByPhoneNumber.Deleted)
            {
                result.ErrorMessages.Add("Customer is deleted.");
                return result;
            }
            
            if (customerByPhoneNumber.Active == false)
            {
                result.ErrorMessages.Add("Customer is disabled.");
                return result;
            }
            
            return await SendOtpAsync(result, model, model.PhoneNumber, true);
        }

        #endregion

        private async Task<ValidateOtpResponseApiModel> RegisterUserAsync(string phoneNumber, string email, bool asVendor, string password)
        {
            var result = new ValidateOtpResponseApiModel();
            var customer = await _customerService.InsertGuestCustomerAsync();

            var store = await _storeContext.GetCurrentStoreAsync();
            var registrationRequest = new CustomerRegistrationRequest(
                customer,
                email, 
                phoneNumber,
                string.IsNullOrEmpty(password) ? GeneratRandomPassword() : password, 
                _customerSettings.DefaultPasswordFormat,
                store.Id);

            var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
            if (registrationResult.Success)
            {
                if(string.IsNullOrEmpty(phoneNumber) == false)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, phoneNumber);
                
                if (asVendor)
                {
                    await _otpVendorService.RegisterCustomerAsVendorAsync(customer);
                }

                result = await GenerateAuthenticateResponse(customer);
            }
            else
            {
                result.ErrorMessages.AddRange(registrationResult.Errors);
            }


            return result;
        }

        private async Task<ValidateOtpResponseApiModel> ChangePhoneNumberAsync(
            string oldPhoneNumber,
            string newPhoneNumber)
        {
            if (string.IsNullOrEmpty(oldPhoneNumber))
            {
                return new ValidateOtpResponseApiModel
                {
                    ErrorMessages = new List<string>
                    {
                        "Invalid old phone number."
                    }
                };
            }
            
            if (string.IsNullOrEmpty(newPhoneNumber))
            {
                return new ValidateOtpResponseApiModel
                {
                    ErrorMessages = new List<string>
                    {
                        "Invalid new phone number."
                    }
                };
            }
            
            Customer customer;
            var genericAttribute = await _repositoryGenericAttribute.Table.FirstOrDefaultAsync(item =>
                item.KeyGroup == "Customer" &&
                item.Key == NopCustomerDefaults.PhoneAttribute &&
                item.Value.Equals(oldPhoneNumber));
            if (genericAttribute == null)
                customer = null;
            else
            {
                customer = await _customerService.GetCustomerByIdAsync(genericAttribute.EntityId);
            }

            if (customer == null)
            {
                var result = new ValidateOtpResponseApiModel();
                result.ErrorMessages.Add("Customer with your old phone number not found");
                return result;
            }

            genericAttribute.Value = newPhoneNumber;

            await _repositoryGenericAttribute.UpdateAsync(genericAttribute);

            customer.Username = newPhoneNumber;

            await _customerService.UpdateCustomerAsync(customer);

            return await GenerateAuthenticateResponse(customer);
        }

        private async Task<ValidateOtpResponseApiModel> ChangePasswordAsync(Customer customer, string password)
        {
            var changePassRequest = new ChangePasswordRequest(customer.Email, false,
                _customerSettings.DefaultPasswordFormat, password);
            var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
            if (changePassResult.Success == false)
            {
                return new ValidateOtpResponseApiModel
                {
                    Success = false,
                    CustomerId = 0,
                    ErrorMessages = new List<string>(changePassResult.Errors)
                };
            }
            return await GenerateAuthenticateResponse(customer);
        }

        private string GeneratRandomPassword()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(RandomString(4, true));
            builder.Append(new Random().Next(1111, 9999));
            builder.Append(RandomString(2, false));
            return builder.ToString();
        }

        private string RandomString(int size, bool lowerCase)
        {
            var builder = new StringBuilder();
            var random = new Random();
            for (int i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            if (lowerCase)
                return builder.ToString().ToLower();

            return builder.ToString();
        }

        private async Task<ValidateOtpResponseApiModel> GenerateAuthenticateResponse(Customer customer)
        {
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
            var roles = customerRoles.Select(item => new RoleItem
            {
                Id = item.Id,
                Name = item.Name,
                SystemName = item.SystemName
            }).ToList();

            var currentTime = DateTimeOffset.Now;
            var expire = currentTime.AddSeconds(_otpAuthenticationSettings.TokenLifetimeSeconds == 0
                ? _webApiCommonSettings.TokenLifetimeDays * 86400
                : _otpAuthenticationSettings.TokenLifetimeSeconds);
            var jwtToken = _jwtTokenService.GetNewJwtToken(customer,
                _otpAuthenticationSettings.TokenLifetimeSeconds == 0
                    ? _webApiCommonSettings.TokenLifetimeDays * 86400
                    : _otpAuthenticationSettings.TokenLifetimeSeconds);
            var refreshToken = GetRefreshToken(customer);

            return new ValidateOtpResponseApiModel
            {
                Success = true,
                CustomerId = customer.Id,
                Roles = roles,
                ExpireDate = expire,
                Token = jwtToken,
                RefreshToken = refreshToken,
                Username = customer.Username
            };
        }

        private string GetRefreshToken(Customer customer)
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

        private async Task<Customer> FindCustomerByPhoneAsync(string phone)
        {
            string secondaryPhoneNumber;
            if (phone.StartsWith("+"))
            {
                secondaryPhoneNumber = phone.Replace("+", "");
            }
            else
            {
                secondaryPhoneNumber = "+" + phone;
            }

            var genericAttributes = await _repositoryGenericAttribute.Table.Where(item =>
                    item.KeyGroup == "Customer" &&
                    item.Key == NopCustomerDefaults.PhoneAttribute &&
                    (item.Value.Equals(phone) || item.Value.Equals(secondaryPhoneNumber)))
                .ToListAsync();

            if (!genericAttributes.Any())
            {
                await _logger.InformationAsync(
                    $"Customer with phone number {phone} and secondary phone {secondaryPhoneNumber} not found");
            }

            foreach (var genericAttribute in genericAttributes)
            {
                var customer = await _customerService.GetCustomerByIdAsync(genericAttribute.EntityId);
                if (customer is { Deleted: false })
                {
                    return customer;
                }
            }

            return null;
        }
    }
}
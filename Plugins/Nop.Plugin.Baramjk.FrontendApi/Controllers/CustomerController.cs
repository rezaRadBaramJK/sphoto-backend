using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.FrontendApi.Dto;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Customer;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class CustomerController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public CustomerController(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAuthenticationService authenticationService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerAttributeService customerAttributeService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            ILogger logger,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IOrderService orderService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            TaxSettings taxSettings, IRewardPointService rewardPointService,
            IRepository<GenericAttribute> genericAttributeRepository,
            IAuthorizationUserService authorizationUserService,
            PasswordRecoveryStrategyResolver passwordRecoveryStrategyResolver,
            IDispatcherService dispatcherService)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _exportManager = exportManager;
            _externalAuthenticationService = externalAuthenticationService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _logger = logger;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _notificationService = notificationService;
            _orderService = orderService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _mediaSettings = mediaSettings;
            _taxSettings = taxSettings;
            _rewardPointService = rewardPointService;
            _genericAttributeRepository = genericAttributeRepository;
            _authorizationUserService = authorizationUserService;
            _passwordRecoveryStrategyResolver = passwordRecoveryStrategyResolver;
            _dispatcherService = dispatcherService;
        }

        #endregion

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetRewardPoint()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var balance = await _rewardPointService.GetRewardPointsBalanceAsync(customer.Id, 0);

            return ApiResponseFactory.Success(balance);
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerInfoModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomersInfo([FromBody] int[] ids)
        {
            var infoDtos = new List<CustomerInfoDto>();
            var customers = await _customerService.GetCustomersByIdsAsync(ids);
            foreach (var customer in customers)
            {
                var model = new CustomerInfoModel();
                model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, false);

                var infoDto = model.ToDto<CustomerInfoDto>();
                infoDto.RegisterDateTimeOnUtc = customer.CreatedOnUtc;

                var attribute = await _genericAttributeService.GetAttributeAsync<int>(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute);

                infoDto.AvatarUrl = await _pictureService.GetPictureUrlAsync(attribute,
                    _mediaSettings.AvatarPictureSize, false);

                infoDtos.Add(infoDto);
            }

            return ApiResponseFactory.Success(infoDtos);
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerInfoModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCustomersInfo([FromQuery] int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            var model = new CustomerInfoModel();
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, false);
            var infoDto = model.ToDto<CustomerInfoDto>();
            infoDto.RegisterDateTimeOnUtc = customer.CreatedOnUtc;

            var attribute = await _genericAttributeService.GetAttributeAsync<int>(customer,
                NopCustomerDefaults.AvatarPictureIdAttribute);

            infoDto.AvatarUrl = await _pictureService.GetPictureUrlAsync(attribute,
                _mediaSettings.AvatarPictureSize, false);

            infoDto.BillingAddressId = customer.BillingAddressId;
            infoDto.ShippingAddressId = customer.ShippingAddressId;

            return ApiResponseFactory.Success(infoDto);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerInfoModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> DeleteMyAccount()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _customerService.DeleteCustomerAsync(customer);
            return ApiResponseFactory.Success();
        }

        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IExportManager _exportManager;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IRewardPointService _rewardPointService;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IAuthorizationUserService _authorizationUserService;
        private readonly PasswordRecoveryStrategyResolver _passwordRecoveryStrategyResolver;
        private readonly IDispatcherService _dispatcherService;

        #endregion

        #region Utilities

        /// <summary>
        ///     Get custom address attributes from the passed form
        /// </summary>
        /// <param name="form">Form values</param>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the attributes in XML format
        /// </returns>
        protected virtual async Task<string> ParseCustomAddressAttributesAsync(IDictionary<string, string> form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;

            foreach (var attribute in await _addressAttributeService.GetAllAddressAttributesAsync())
            {
                string attributeValues;

                if (form.ContainsKey(attribute.Name))
                    attributeValues = form[attribute.Name];
                else if (string.Format(NopCommonDefaults.AddressAttributeControlName, attribute.Id) is { } key &&
                         form.ContainsKey(key))
                    attributeValues = form[key];
                else
                    continue;

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        if (!StringValues.IsNullOrEmpty(attributeValues) &&
                            int.TryParse(attributeValues, out var value) && value > 0)
                            attributesXml =
                                _addressAttributeParser.AddAddressAttribute(attributesXml, attribute, value.ToString());
                        break;

                    case AttributeControlType.Checkboxes:
                        foreach (var attributeValue in attributeValues.Split(new[] { ',' },
                                     StringSplitOptions.RemoveEmptyEntries))
                            if (int.TryParse(attributeValue, out value) && value > 0)
                                attributesXml =
                                    _addressAttributeParser.AddAddressAttribute(attributesXml, attribute,
                                        value.ToString());

                        break;

                    case AttributeControlType.ReadonlyCheckboxes:
                        //load read-only (already server-side selected) values
                        var addressAttributeValues =
                            await _addressAttributeService.GetAddressAttributeValuesAsync(attribute.Id);
                        foreach (var addressAttributeValue in addressAttributeValues)
                            if (addressAttributeValue.IsPreSelected)
                                attributesXml = _addressAttributeParser.AddAddressAttribute(attributesXml, attribute,
                                    addressAttributeValue.Id.ToString());

                        break;

                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        if (!StringValues.IsNullOrEmpty(attributeValues))
                            attributesXml =
                                _addressAttributeParser.AddAddressAttribute(attributesXml, attribute,
                                    attributeValues.Trim());
                        break;

                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected virtual void ValidateRequiredConsents(List<GdprConsent> consents, IDictionary<string, string> form)
        {
            foreach (var consent in consents)
            {
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.Equals("on"))
                    ModelState.AddModelError(string.Empty, consent.RequiredMessage);
            }
        }

        protected virtual async Task<string> ParseSelectedProviderAsync(IDictionary<string, string> form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var multiFactorAuthenticationProviders =
                await _multiFactorAuthenticationPluginManager.LoadActivePluginsAsync(
                    await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
            foreach (var provider in multiFactorAuthenticationProviders)
            {
                var controlId = $"provider_{provider.PluginDescriptor.SystemName}";

                var curProvider = form[controlId];
                if (!StringValues.IsNullOrEmpty(curProvider))
                {
                    var selectedProvider = curProvider;
                    if (!string.IsNullOrEmpty(selectedProvider)) return selectedProvider;
                }
            }

            return string.Empty;
        }

        protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IDictionary<string, string> form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;
            var attributes = await _customerAttributeService.GetAllCustomerAttributesAsync();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                        break;
                    case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                            foreach (var item in cblAttributes.Split(new[] { ',' },
                                         StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                    }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues =
                            await _customerAttributeService.GetCustomerAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                                     .Where(v => v.IsPreSelected)
                                     .Select(v => v.Id)
                                     .ToList())
                            attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.Trim();
                            attributesXml = _customerAttributeParser.AddCustomerAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }

                        break;
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    //not supported customer attributes
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected virtual async Task LogGdprAsync(Customer customer, CustomerInfoModel oldCustomerInfoModel,
            CustomerInfoModel newCustomerInfoModel, IDictionary<string, string> form)
        {
            try
            {
                //consents
                var consents = (await _gdprService.GetAllConsentsAsync())
                    .Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
                foreach (var consent in consents)
                {
                    var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id,
                        (await _workContext.GetCurrentCustomerAsync()).Id);
                    var controlId = $"consent{consent.Id}";
                    var cbConsent = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.Equals("on"))
                    {
                        //agree
                        if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree,
                                consent.Message);
                    }
                    else
                    {
                        //disagree
                        if (!previousConsentValue.HasValue || previousConsentValue.Value)
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree,
                                consent.Message);
                    }
                }

                //newsletter subscriptions
                if (_gdprSettings.LogNewsletterConsent)
                {
                    if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree,
                            await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                    if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree,
                            await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                }

                //user profile changes
                if (!_gdprSettings.LogUserProfileChanges)
                    return;

                if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

                if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

                if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

                if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");

                if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

                if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

                if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

                if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

                if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

                if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

                if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

                if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
                {
                    var countryName = (await _countryService.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
                }

                if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
                {
                    var stateProvinceName =
                        (await _stateProvinceService.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))
                        ?.Name;
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged,
                        $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
                }
            }
            catch (Exception exception)
            {
                await _logger.ErrorAsync(exception.Message, exception, customer);
            }
        }

        #endregion

        #region Methods

        #region Login / logout

        /// <summary>
        ///     Logout
        /// </summary>
        [HttpGet]
        public virtual async Task<IActionResult> Logout()
        {
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //activity log
                await _customerActivityService.InsertActivityAsync(_workContext.OriginalCustomerIfImpersonated,
                    "Impersonation.Finished",
                    string.Format(
                        await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.StoreOwner"),
                        (await _workContext.GetCurrentCustomerAsync()).Email,
                        (await _workContext.GetCurrentCustomerAsync()).Id),
                    await _workContext.GetCurrentCustomerAsync());

                await _customerActivityService.InsertActivityAsync("Impersonation.Finished",
                    string.Format(
                        await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.Customer"),
                        _workContext.OriginalCustomerIfImpersonated.Email,
                        _workContext.OriginalCustomerIfImpersonated.Id),
                    _workContext.OriginalCustomerIfImpersonated);

                //logout impersonated customer
                await _genericAttributeService
                    .SaveAttributeAsync<int?>(_workContext.OriginalCustomerIfImpersonated,
                        NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);
            }

            //activity log
            await _customerActivityService.InsertActivityAsync(await _workContext.GetCurrentCustomerAsync(),
                "PublicStore.Logout",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Logout"),
                await _workContext.GetCurrentCustomerAsync());

            //standard logout 
            await _authenticationService.SignOutAsync();

            //raise logged out event       
            await _eventPublisher.PublishAsync(
                new CustomerLoggedOutEvent(await _workContext.GetCurrentCustomerAsync()));

            ////EU Cookie
            //if (_storeInformationSettings.DisplayEuCookieLawWarning)
            //{
            //    //the cookie law message should not pop up immediately after logout.
            //    //otherwise, the user will have to click it again...
            //    //and thus next visitor will not click it... so violation for that cookie law..
            //    //the only good solution in this case is to store a temporary variable
            //    //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
            //    //but it'll be displayed for further page loads
            //    TempData[$"{NopCookieDefaults.Prefix}{NopCookieDefaults.IgnoreEuCookieLawWarning}"] = true;
            //}

            return ApiResponseFactory.Success();
        }

        #endregion

        #region Password recovery

        /// <summary>
        ///     Prepare the password recovery model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PasswordRecoveryModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PasswordRecovery()
        {
            var model = new PasswordRecoveryModel();
            model = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);

            return ApiResponseFactory.Success(model.ToDto<PasswordRecoveryModelDto>());
        }

        /// <summary>
        ///     Password recovery send
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(PasswordRecoveryModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PasswordRecoverySend([FromBody] PasswordRecoveryModelDto model)
        {
            var passwordRecoveryModel = model.FromDto<PasswordRecoveryModel>();
            var result = await _passwordRecoveryStrategyResolver.Send(passwordRecoveryModel);

            return ApiResponseFactory.Auto(result.Success, result.PasswordRecoveryModel.ToDto<PasswordRecoveryModelDto>());
        }

        /// <summary>
        ///     Password recovery confirm
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PasswordRecoveryConfirmModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PasswordRecoveryConfirm([FromQuery] [Required] string token,
            [FromQuery] [Required] string email, [FromQuery] [Required] Guid customerGuid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email)
                           ?? await _customerService.GetCustomerByGuidAsync(customerGuid);

            if (customer == null)
                return ApiResponseFactory.NotFound($"Customer by guid={customerGuid} or email={email} not found.");

            var model = new PasswordRecoveryConfirmModel { ReturnUrl = Url.RouteUrl("Homepage") };
            if (string.IsNullOrEmpty(await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.PasswordRecoveryTokenAttribute)))
            {
                model.DisablePasswordChanging = true;
                model.Result =
                    await _localizationService.GetResourceAsync(
                        "Account.PasswordRecovery.PasswordAlreadyHasBeenChanged");
                return ApiResponseFactory.Success(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            //validate token
            if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken");
                return ApiResponseFactory.Success(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            //validate token expiration date
            if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired");
                return ApiResponseFactory.Success(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            return ApiResponseFactory.Success(model.ToDto<PasswordRecoveryConfirmModelDto>());
        }

        /// <summary>
        ///     Password recovery confirm post
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PasswordRecoveryConfirmModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PasswordRecoveryConfirmPOST(
            [FromBody] PasswordRecoveryConfirmModelDto passwordRecoveryConfirmModel,
            [FromQuery] [Required] string token,
            [FromQuery] string email,
            [FromQuery] string userName,
            [FromQuery] Guid? customerGuid)
        {
            Customer customer = null;

            if (string.IsNullOrEmpty(email) == false)
                customer = await _customerService.GetCustomerByEmailAsync(email);
            if (string.IsNullOrEmpty(userName) == false)
                customer = await _customerService.GetCustomerByUsernameAsync(userName);
            else if (customerGuid != null)
                customer = await _customerService.GetCustomerByGuidAsync(customerGuid.Value);

            if (customer == null)
                return ApiResponseFactory.NotFound("Customer not found.");

            var model = passwordRecoveryConfirmModel.FromDto<PasswordRecoveryConfirmModelDto>();

            model.ReturnUrl = Url.RouteUrl("Homepage");

            //validate token
            if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken");
                return ApiResponseFactory.BadRequest(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            //validate token expiration date
            if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
            {
                model.DisablePasswordChanging = true;
                model.Result = await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired");
                return ApiResponseFactory.BadRequest(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            var response = await _customerRegistrationService
                .ChangePasswordAsync(new ChangePasswordRequest(customer.Email, false,
                    _customerSettings.DefaultPasswordFormat, model.NewPassword));
            if (!response.Success)
            {
                model.Result = string.Join(';', response.Errors);
                return ApiResponseFactory.BadRequest(model.ToDto<PasswordRecoveryConfirmModelDto>());
            }

            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.PasswordRecoveryTokenAttribute, string.Empty);

            //authenticate customer after changing password
            await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

            model.DisablePasswordChanging = true;
            model.Result =
                await _localizationService.GetResourceAsync("Account.PasswordRecovery.PasswordHasBeenChanged");
            return ApiResponseFactory.Success(model.ToDto<PasswordRecoveryConfirmModelDto>());
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PasswordRecoveryConfirmModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> VerifyPasswordRecoveryToken(
            [FromQuery] [Required] string token,
            [FromQuery] string email,
            [FromQuery] string userName,
            [FromQuery] Guid? customerGuid)
        {
            Customer customer = null;

            if (string.IsNullOrEmpty(email) == false)
                customer = await _customerService.GetCustomerByEmailAsync(email);
            if (string.IsNullOrEmpty(userName) == false)
                customer = await _customerService.GetCustomerByUsernameAsync(userName);
            else if (customerGuid != null)
                customer = await _customerService.GetCustomerByGuidAsync(customerGuid.Value);

            if (customer == null)
                return ApiResponseFactory.NotFound("Customer not found.");

            //validate token
            if (!await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token))
            {
                var msg = await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken");
                return ApiResponseFactory.BadRequest(msg);
            }

            if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
            {
                var msg = await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired");
                return ApiResponseFactory.BadRequest(msg);
            }

            return ApiResponseFactory.Success();
        }

        #endregion

        #region Register

        /// <summary>
        ///     Prepare the customer register model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RegisterModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Register()
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return ApiResponseFactory.NotFound($"User registration type={UserRegistrationType.Disabled}");

            var model = new RegisterModel();
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, false, setDefaultValues: true);

            return ApiResponseFactory.Success(model.ToDto<RegisterModelDto>());
        }

        /// <summary>
        ///     Register
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RegisterModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Register([FromBody] BaseModelDtoRequest<RegisterModelDto> request,
            [FromQuery] [Required] string returnUrl)
        {
            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return ApiResponseFactory.NotFound($"User registration type={UserRegistrationType.Disabled}");

            if (await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                //Already registered customer. 
                await _authenticationService.SignOutAsync();

                //raise logged out event       
                await _eventPublisher.PublishAsync(
                    new CustomerLoggedOutEvent(await _workContext.GetCurrentCustomerAsync()));

                //Save a new record
                await _workContext.SetCurrentCustomerAsync(await _customerService.InsertGuestCustomerAsync());
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            customer.RegisteredInStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var errors = new List<string>();

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(request.Form);
            var customerAttributeWarnings =
                await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            errors.AddRange(customerAttributeWarnings);

            if (errors.Any())
                return ApiResponseFactory.BadRequest(null, errors);

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                        .GetAllConsentsAsync())
                    .Where(consent => consent.DisplayDuringRegistration && consent.IsRequired)
                    .ToList();

                ValidateRequiredConsents(consents, request.Form);
            }

            var model = request.Model.FromDto<RegisterModel>();

            if (string.IsNullOrEmpty(model.Phone) == false)
            {
                var exists = _genericAttributeRepository.Table.Any(item =>
                    item.KeyGroup == "Customer" &&
                    item.Key == NopCustomerDefaults.PhoneAttribute &&
                    item.Value == model.Phone);

                if (exists)
                {
                    errors.Add("Phone number is exists");
                    return ApiResponseFactory.BadRequest(null, errors);
                }
            }

            var customerUserName = model.Username?.Trim();
            var customerEmail = model.Email?.Trim();

            var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
            var registrationRequest = new CustomerRegistrationRequest(customer,
                customerEmail,
                customerUserName,
                model.Password,
                _customerSettings.DefaultPasswordFormat,
                (await _storeContext.GetCurrentStoreAsync()).Id,
                isApproved);
            var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
            if (registrationResult.Success)
            {
                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.TimeZoneIdAttribute,
                        model.TimeZoneId);
                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.VatNumberAttribute,
                        model.VatNumber);

                    var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                    //send VAT number admin notification
                    if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                        await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                            model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.GenderAttribute,
                        model.Gender);
                if (_customerSettings.FirstNameEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute,
                        model.FirstName);
                if (_customerSettings.LastNameEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute,
                        model.LastName);
                if (_customerSettings.DateOfBirthEnabled)
                {
                    var dateOfBirth = model.ParseDateOfBirth();
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                }

                if (_customerSettings.CompanyEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CompanyAttribute,
                        model.Company);
                if (_customerSettings.StreetAddressEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                if (_customerSettings.StreetAddress2Enabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                if (_customerSettings.ZipPostalCodeEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                if (_customerSettings.CityEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CityAttribute,
                        model.City);
                if (_customerSettings.CountyEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CountyAttribute,
                        model.County);
                if (_customerSettings.CountryEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CountryIdAttribute,
                        model.CountryId);
                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.StateProvinceIdAttribute,
                        model.StateProvinceId);
                if (_customerSettings.PhoneEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute,
                        model.Phone);
                if (_customerSettings.FaxEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FaxAttribute,
                        model.Fax);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    var isNewsletterActive =
                        _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;

                    //save newsletter value
                    var newsletter =
                        await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(
                            customerEmail, (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (newsletter != null)
                    {
                        if (model.Newsletter)
                        {
                            newsletter.Active = isNewsletterActive;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree,
                                    await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                        }
                        //else
                        //{
                        //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                        //_newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);
                        //}
                    }
                    else
                    {
                        if (model.Newsletter)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(
                                new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customerEmail,
                                    Active = isNewsletterActive,
                                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });

                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree,
                                    await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                        }
                    }
                }

                if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    //privacy policy is required
                    //GDPR
                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree,
                            await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));

                //GDPR
                if (_gdprSettings.GdprEnabled)
                {
                    var consents = (await _gdprService.GetAllConsentsAsync())
                        .Where(consent => consent.DisplayDuringRegistration).ToList();
                    foreach (var consent in consents)
                    {
                        var controlId = $"consent{consent.Id}";
                        var cbConsent = request.Form[controlId];
                        if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.Equals("on"))
                            //agree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree,
                                consent.Message);
                        else
                            //disagree
                            await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree,
                                consent.Message);
                    }
                }

                //save customer attributes
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                //insert default address (if possible)
                var defaultAddress = new Address
                {
                    FirstName =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.FirstNameAttribute),
                    LastName =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.LastNameAttribute),
                    Email = customer.Email,
                    Company =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.CompanyAttribute),
                    CountryId = await _genericAttributeService.GetAttributeAsync<int>(customer,
                        NopCustomerDefaults.CountryIdAttribute) > 0
                        ? await _genericAttributeService.GetAttributeAsync<int>(customer,
                            NopCustomerDefaults.CountryIdAttribute)
                        : null,
                    StateProvinceId =
                        await _genericAttributeService.GetAttributeAsync<int>(customer,
                            NopCustomerDefaults.StateProvinceIdAttribute) > 0
                            ? await _genericAttributeService.GetAttributeAsync<int>(customer,
                                NopCustomerDefaults.StateProvinceIdAttribute)
                            : null,
                    County =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.CountyAttribute),
                    City =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.CityAttribute),
                    Address1 =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.StreetAddressAttribute),
                    Address2 =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.StreetAddress2Attribute),
                    ZipPostalCode =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.ZipPostalCodeAttribute),
                    PhoneNumber =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.PhoneAttribute),
                    FaxNumber = await _genericAttributeService.GetAttributeAsync<string>(customer,
                        NopCustomerDefaults.FaxAttribute),
                    CreatedOnUtc = customer.CreatedOnUtc
                };
                if (await _addressService.IsAddressValidAsync(defaultAddress))
                {
                    //some validation
                    if (defaultAddress.CountryId == 0)
                        defaultAddress.CountryId = null;
                    if (defaultAddress.StateProvinceId == 0)
                        defaultAddress.StateProvinceId = null;
                    //set default address
                    //customer.Addresses.Add(defaultAddress);

                    await _addressService.InsertAddressAsync(defaultAddress);

                    await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                    customer.BillingAddressId = defaultAddress.Id;
                    customer.ShippingAddressId = defaultAddress.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }

                //notifications
                if (_customerSettings.NotifyNewCustomerRegistration)
                    await _workflowMessageService.SendCustomerRegisteredNotificationMessageAsync(customer,
                        _localizationSettings.DefaultAdminLanguageId);

                //raise event       
                await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));

                switch (_customerSettings.UserRegistrationType)
                {
                    case UserRegistrationType.EmailValidation:
                    {
                        var token = new Random().Next(12345, 99999);
                        await _genericAttributeService.SaveAttributeAsync(customer,
                            NopCustomerDefaults.AccountActivationTokenAttribute, token);
                        await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer,
                            (await _workContext.GetWorkingLanguageAsync()).Id);
                        break;
                    }
                    case UserRegistrationType.AdminApproval:
                        return ApiResponseFactory.NotFound(
                            $"This registration type '{nameof(UserRegistrationType.AdminApproval)}' is not supported for Web API.");

                    case UserRegistrationType.Standard:
                        //send customer welcome message
                        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer,
                            (await _workContext.GetWorkingLanguageAsync()).Id);

                        //raise event       
                        await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                        //If we got this far, something failed, redisplay form
                        var response = await _authorizationUserService.AuthenticateAsync(customer);
                        return ApiResponseFactory.Success(response);

                    default:
                        return ApiResponseFactory.BadRequest(new List<string>
                            { $"Unknow {_customerSettings.UserRegistrationType}" });
                }
            }

            //errors
            if (registrationResult.Errors.Any())
                return ApiResponseFactory.BadRequest(null, registrationResult.Errors);

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareRegisterModelAsync(model, true, customerAttributesXml);

            return ApiResponseFactory.Success(model.ToDto<RegisterModelDto>());
        }

        /// <summary>
        ///     Prepare the register result model
        /// </summary>
        [HttpPost("{resultId}")]
        [ProducesResponseType(typeof(RegisterResultModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RegisterResult(int resultId, [FromQuery] [Required] string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            var model = await _customerModelFactory.PrepareRegisterResultModelAsync(resultId, returnUrl);
            return ApiResponseFactory.Success(model.ToDto<RegisterResultModelDto>());
        }

        /// <summary>
        ///     Check Username availability
        /// </summary>
        /// <param name="username">Username</param>
        [HttpGet]
        [ProducesResponseType(typeof(CheckUsernameAvailabilityResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckUsernameAvailability([FromQuery] [Required] string username)
        {
            var usernameAvailable = false;
            var statusText =
                await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.NotAvailable");

            if (!UsernamePropertyValidator.IsValid(username, _customerSettings))
            {
                statusText = await _localizationService.GetResourceAsync("Account.Fields.Username.NotValid");
            }
            else if (_customerSettings.UsernamesEnabled && !string.IsNullOrWhiteSpace(username))
            {
                if (await _workContext.GetCurrentCustomerAsync() != null &&
                    (await _workContext.GetCurrentCustomerAsync()).Username != null &&
                    (await _workContext.GetCurrentCustomerAsync()).Username.Equals(username,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    statusText =
                        await _localizationService.GetResourceAsync(
                            "Account.CheckUsernameAvailability.CurrentUsername");
                }
                else
                {
                    var customer = await _customerService.GetCustomerByUsernameAsync(username);
                    if (customer == null)
                    {
                        statusText =
                            await _localizationService.GetResourceAsync("Account.CheckUsernameAvailability.Available");
                        usernameAvailable = true;
                    }
                }
            }

            return ApiResponseFactory.Success(new CheckUsernameAvailabilityResponse
                { Available = usernameAvailable, Text = statusText });
        }

        /// <summary>
        ///     Account activation
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AccountActivationModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AccountActivation([FromQuery] [Required] string token,
            [FromQuery] string email, [FromQuery] Guid customerGuid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email)
                           ?? await _customerService.GetCustomerByGuidAsync(customerGuid);

            if (customer == null)
            {
                var customerId = (await _genericAttributeRepository.Table.FirstOrDefaultAsync(item =>
                    item.Key == NopCustomerDefaults.AccountActivationTokenAttribute &&
                    item.Value == token))?.EntityId;

                if (customerId != null)
                    customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
            }

            if (customer == null)
                return ApiResponseFactory.NotFound($"Customer by guid={customerGuid} or email={email} not found.");

            var model = new AccountActivationModel { ReturnUrl = Url.RouteUrl("Homepage") };
            var cToken = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.AccountActivationTokenAttribute);
            if (string.IsNullOrEmpty(cToken))
            {
                model.Result =
                    await _localizationService.GetResourceAsync("Account.AccountActivation.AlreadyActivated");
                return ApiResponseFactory.Success(model.ToDto<AccountActivationModelDto>());
            }

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return ApiResponseFactory.BadRequest("No match was found for the token for the specified customer.");

            //activate user account
            customer.Active = true;
            await _customerService.UpdateCustomerAsync(customer);
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.AccountActivationTokenAttribute, string.Empty);

            //send welcome message
            await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer,
                (await _workContext.GetWorkingLanguageAsync()).Id);

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

            //authenticate customer after activation
            await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

            //activating newsletter if need
            var newsletter =
                await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email,
                    (await _storeContext.GetCurrentStoreAsync()).Id);
            if (newsletter != null && !newsletter.Active)
            {
                newsletter.Active = true;
                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
            }

            model.Result = await _localizationService.GetResourceAsync("Account.AccountActivation.Activated");
            return ApiResponseFactory.Success(model.ToDto<AccountActivationModelDto>());
        }

        #endregion

        #region My account / Info

        /// <summary>
        ///     Prepare the customer info model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerInfoModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Info()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            var model = new CustomerInfoModel();
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, false);

            var infoModelDto = model.ToDto<CustomerInfoModelDto>();
            infoModelDto.AvailableTimeZones = null;
            infoModelDto.RegisterDateTimeOnUtc = customer.CreatedOnUtc;

            var roles = (await _customerService.GetCustomerRolesAsync(customer))
                .Select(item => item.Name)
                .ToList();

            var attribute = await _genericAttributeService.GetAttributeAsync<int>(customer,
                NopCustomerDefaults.AvatarPictureIdAttribute);

            infoModelDto.AvatarUrl = await _pictureService.GetPictureUrlAsync(attribute,
                _mediaSettings.AvatarPictureSize, false);

            infoModelDto.Roles = roles;
            infoModelDto.Id = customer.Id;

            return ApiResponseFactory.Success(infoModelDto);
        }

        /// <summary>
        ///     Customer info
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(InfoResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Info([FromBody] BaseModelDtoRequest<CustomerInfoModelDto> request)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            var oldCustomerModel = new CustomerInfoModel();

            var customer = await _workContext.GetCurrentCustomerAsync();

            //get customer info model before changes for gdpr log
            if (_gdprSettings.GdprEnabled & _gdprSettings.LogUserProfileChanges)
                oldCustomerModel =
                    await _customerModelFactory.PrepareCustomerInfoModelAsync(oldCustomerModel, customer, false);

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(request.Form);
            var customerAttributeWarnings =
                await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);

            var errors = new List<string>();
            errors.AddRange(customerAttributeWarnings);

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                        .GetAllConsentsAsync())
                    .Where(consent => consent.DisplayOnCustomerInfoPage && consent.IsRequired)
                    .ToList();

                ValidateRequiredConsents(consents, request.Form);
            }

            var model = request.Model.FromDto<CustomerInfoModel>();

            try
            {
                //username 
                if (_customerSettings.UsernamesEnabled && _customerSettings.AllowUsersToChangeUsernames)
                {
                    var userName = model.Username.Trim();
                    if (!customer.Username.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change username
                        await _customerRegistrationService.SetUsernameAsync(customer, userName);

                        //re-authenticate
                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                            await _authenticationService.SignInAsync(customer, true);
                    }
                }

                if (string.IsNullOrEmpty(model.Email) == false)
                {
                    //email
                    var email = model.Email.Trim();
                    if (!customer.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //change email
                        var requireValidation =
                            _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
                        await _customerRegistrationService.SetEmailAsync(customer, email, requireValidation);

                        //do not authenticate users in impersonation mode
                        if (_workContext.OriginalCustomerIfImpersonated == null)
                            //re-authenticate (if usernames are disabled)
                            if (!_customerSettings.UsernamesEnabled && !requireValidation)
                                await _authenticationService.SignInAsync(customer, true);
                    }
                }

                //properties
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.TimeZoneIdAttribute,
                        model.TimeZoneId);
                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    var prevVatNumber =
                        await _genericAttributeService.GetAttributeAsync<string>(customer,
                            NopCustomerDefaults.VatNumberAttribute);

                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.VatNumberAttribute,
                        model.VatNumber);
                    if (prevVatNumber != model.VatNumber)
                    {
                        var (vatNumberStatus, _, vatAddress) =
                            await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                        await _genericAttributeService.SaveAttributeAsync(customer,
                            NopCustomerDefaults.VatNumberStatusIdAttribute, (int)vatNumberStatus);
                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer,
                                model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }
                }

                //form fields
                if (_customerSettings.GenderEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.GenderAttribute,
                        model.Gender);
                if (_customerSettings.FirstNameEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute,
                        model.FirstName);
                if (_customerSettings.LastNameEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute,
                        model.LastName);
                if (_customerSettings.DateOfBirthEnabled)
                {
                    var dateOfBirth = model.ParseDateOfBirth();
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                }

                if (_customerSettings.CompanyEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CompanyAttribute,
                        model.Company);
                if (_customerSettings.StreetAddressEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.StreetAddressAttribute, model.StreetAddress);
                if (_customerSettings.StreetAddress2Enabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.StreetAddress2Attribute, model.StreetAddress2);
                if (_customerSettings.ZipPostalCodeEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.ZipPostalCodeAttribute, model.ZipPostalCode);
                if (_customerSettings.CityEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CityAttribute,
                        model.City);
                if (_customerSettings.CountyEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CountyAttribute,
                        model.County);
                if (_customerSettings.CountryEnabled)
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CountryIdAttribute,
                        model.CountryId);

                    var customerCountryMapping = new Dictionary<int, int>();
                    customerCountryMapping.TryAdd(customer.Id, model.CountryId);
                    
                    await _dispatcherService.PublishAsync("CustomerCountryChangedEvent", customerCountryMapping);
                }

                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.StateProvinceIdAttribute, model.StateProvinceId);
                if (_customerSettings.PhoneEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute,
                        model.Phone);
                if (_customerSettings.FaxEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FaxAttribute,
                        model.Fax);

                //newsletter
                if (_customerSettings.NewsletterEnabled)
                {
                    //save newsletter value
                    var newsletter =
                        await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(
                            customer.Email, (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (newsletter != null)
                    {
                        if (model.Newsletter)
                        {
                            newsletter.Active = true;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);
                        }
                        else
                        {
                            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletter);
                        }
                    }
                    else
                    {
                        if (model.Newsletter)
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(
                                new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                    }
                }

                if (_forumSettings.ForumsEnabled && _forumSettings.SignaturesEnabled)
                    await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SignatureAttribute,
                        model.Signature);

                //save customer attributes
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.CustomCustomerAttributes, customerAttributesXml);

                //GDPR
                if (_gdprSettings.GdprEnabled)
                    await LogGdprAsync(customer, oldCustomerModel, model, request.Form);

                return ApiResponseFactory.Success(new InfoResponse
                {
                    Errors = errors,
                    Model = model.ToDto<CustomerInfoModelDto>()
                });
            }
            catch (Exception exc)
            {
                errors.Add(exc.Message);
                await _logger.ErrorAsync(exc.Message, exc);
            }

            //If we got this far, something failed, redisplay form
            model = await _customerModelFactory.PrepareCustomerInfoModelAsync(model, customer, true,
                customerAttributesXml);
            var modelDto = model.ToDto<CustomerInfoModelDto>();
            modelDto.RegisterDateTimeOnUtc = customer.CreatedOnUtc;

            return ApiResponseFactory.Success(new InfoResponse
            {
                Errors = errors,
                Model = modelDto
            });
        }

        /// <summary>
        ///     Delete the external authentication record
        /// </summary>
        /// <param name="id">External authentication record identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveExternalAssociation(int id)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            //ensure it's our record
            var ear = await _externalAuthenticationService.GetExternalAuthenticationRecordByIdAsync(id);

            if (ear == null)
                return ApiResponseFactory.Success(Url.Action("Info"));

            await _externalAuthenticationService.DeleteExternalAuthenticationRecordAsync(ear);

            return ApiResponseFactory.Success(Url.Action("Info"));
        }

        /// <summary>
        ///     Email revalidation
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EmailRevalidationModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> EmailRevalidation([FromQuery] [Required] string token,
            [FromQuery] [Required] string email,
            [FromQuery] [Required] Guid customerGuid)
        {
            //For backward compatibility with previous versions where email was used as a parameter in the URL
            var customer = await _customerService.GetCustomerByEmailAsync(email)
                           ?? await _customerService.GetCustomerByGuidAsync(customerGuid);

            if (customer == null)
                return ApiResponseFactory.NotFound($"Customer by guid={customerGuid} or email={email} not found.");

            var model = new EmailRevalidationModel { ReturnUrl = Url.RouteUrl("Homepage") };
            var cToken = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.EmailRevalidationTokenAttribute);
            if (string.IsNullOrEmpty(cToken))
            {
                model.Result = await _localizationService.GetResourceAsync("Account.EmailRevalidation.AlreadyChanged");
                return ApiResponseFactory.Success(model.ToDto<EmailRevalidationModelDto>());
            }

            if (!cToken.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                return ApiResponseFactory.BadRequest("No match was found for the token for the specified customer.");

            if (string.IsNullOrEmpty(customer.EmailToRevalidate))
                return ApiResponseFactory.NotFound("Email to revalidate is not found for current customer.");

            if (_customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation)
                return ApiResponseFactory.BadRequest(
                    $"The setting {nameof(_customerSettings.UserRegistrationType)} is not equal {UserRegistrationType.EmailValidation}");

            //change email
            try
            {
                await _customerRegistrationService.SetEmailAsync(customer, customer.EmailToRevalidate, false);
            }
            catch (Exception exc)
            {
                model.Result = await _localizationService.GetResourceAsync(exc.Message);
                return ApiResponseFactory.Success(model.ToDto<EmailRevalidationModelDto>());
            }

            customer.EmailToRevalidate = null;
            await _customerService.UpdateCustomerAsync(customer);
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.EmailRevalidationTokenAttribute, string.Empty);

            //authenticate customer after changing email
            await _customerRegistrationService.SignInCustomerAsync(customer, null, true);

            model.Result = await _localizationService.GetResourceAsync("Account.EmailRevalidation.Changed");
            return ApiResponseFactory.Success(model.ToDto<EmailRevalidationModelDto>());
        }

        #endregion

        #region My account / Addresses

        /// <summary>
        ///     Prepare the customer address list model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAddressListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Addresses()
        {
            var model = await _customerModelFactory.PrepareCustomerAddressListModelAsync();

            return ApiResponseFactory.Success(model.ToDto<CustomerAddressListModelDto>());
        }

        /// <summary>
        ///     Address delete
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        [HttpDelete("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressDelete(int addressId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address != null)
            {
                await _customerService.RemoveCustomerAddressAsync(customer, address);
                await _customerService.UpdateCustomerAsync(customer);
                //now delete the address record
                await _addressService.DeleteAddressAsync(address);
            }

            //redirect to the address list page
            return ApiResponseFactory.Success(Url.RouteUrl("CustomerAddresses"));
        }

        /// <summary>
        ///     Prepare address model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAddressEditModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressAdd()
        {
            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                null,
                false,
                _addressSettings,
                async () =>
                    await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            return ApiResponseFactory.Success(model.ToDto<CustomerAddressEditModelDto>());
        }

        /// <summary>
        ///     Address add
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddressAddResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressAdd(
            [FromBody] BaseModelDtoRequest<CustomerAddressEditModelDto> request)
        {
            //custom address attributes
            var customAttributes = request.Form == null ? string.Empty : await ParseCustomAddressAttributesAsync(request.Form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);

            var model = request.Model.FromDto<CustomerAddressEditModel>();

            if (!customAttributeWarnings.Any())
            {
                var address = model.Address.ToEntity();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.InsertAddressAsync(address);

                await _customerService.InsertCustomerAddressAsync(await _workContext.GetCurrentCustomerAsync(),
                    address);
                model.Address.Id = address.Id;
                return ApiResponseFactory.Success(new AddressAddResponse
                {
                    Model = model.ToDto<CustomerAddressEditModelDto>()
                });
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                null,
                true,
                _addressSettings,
                async () =>
                    await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            return ApiResponseFactory.Success(new AddressAddResponse
            {
                Model = model.ToDto<CustomerAddressEditModelDto>(),
                Errors = customAttributeWarnings
            });
        }

        /// <summary>
        ///     Prepare address model
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        [HttpGet("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAddressEditModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressEdit(int addressId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return ApiResponseFactory.NotFound($"Address by id={addressId} is not found.");

            var model = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address,
                false,
                _addressSettings,
                async () =>
                    await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            return ApiResponseFactory.Success(model.ToDto<CustomerAddressEditModelDto>());
        }

        /// <summary>
        ///     Update address
        /// </summary>
        [HttpPut("{addressId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AddressEditResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddressEdit(
            [FromBody] BaseModelDtoRequest<CustomerAddressEditModelDto> request, int addressId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
            if (address == null)
                //address is not found
                return ApiResponseFactory.NotFound($"Address by id={addressId} is not found.");

            //custom address attributes
            var customAttributes = await ParseCustomAddressAttributesAsync(request.Form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);

            var model = request.Model.FromDto<CustomerAddressEditModel>();

            if (!customAttributeWarnings.Any())
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                await _addressService.UpdateAddressAsync(address);

                return ApiResponseFactory.Success(new AddressEditResponse
                {
                    Model = model.ToDto<CustomerAddressEditModelDto>()
                });
            }

            //If we got this far, something failed, redisplay form
            await _addressModelFactory.PrepareAddressModelAsync(model.Address,
                address,
                true,
                _addressSettings,
                async () =>
                    await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                overrideAttributesXml: customAttributes);

            return ApiResponseFactory.Success(new AddressEditResponse
            {
                Model = model.ToDto<CustomerAddressEditModelDto>(),
                Errors = customAttributeWarnings
            });
        }

        #endregion

        #region My account / Downloadable products

        /// <summary>
        ///     Prepare the customer downloadable products model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerDownloadableProductsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> DownloadableProducts()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (_customerSettings.HideDownloadableProductsTab)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_customerSettings.HideDownloadableProductsTab)} is true.");

            var model = await _customerModelFactory.PrepareCustomerDownloadableProductsModelAsync();

            return ApiResponseFactory.Success(model.ToDto<CustomerDownloadableProductsModelDto>());
        }

        /// <summary>
        ///     Prepare the user agreement model
        /// </summary>
        /// <param name="orderItemId">Order item guid identifier</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UserAgreementModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UserAgreement([FromQuery] [Required] Guid orderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByGuidAsync(orderItemId);
            if (orderItem == null)
                return ApiResponseFactory.NotFound($"Order item by guid={orderItemId} is not found.");

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            if (product == null || !product.HasUserAgreement)
                return ApiResponseFactory.NotFound($"Produc by id={orderItem.ProductId} is not found.");

            var model = await _customerModelFactory.PrepareUserAgreementModelAsync(orderItem, product);

            return ApiResponseFactory.Success(model.ToDto<UserAgreementModelDto>());
        }

        #endregion

        #region My account / Change password

        /// <summary>
        ///     Prepare the change password model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ChangePasswordModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ChangePassword()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            var model = await _customerModelFactory.PrepareChangePasswordModelAsync();

            //display the cause of the change password 
            if (await _customerService.PasswordIsExpiredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest(
                    await _localizationService.GetResourceAsync("Account.ChangePassword.PasswordIsExpired"));

            return ApiResponseFactory.Success(model.ToDto<ChangePasswordModelDto>());
        }

        /// <summary>
        ///     Change password
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ChangePasswordModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModelDto model)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest(new List<string> { "Customer is not registered." });

            var customer = await _workContext.GetCurrentCustomerAsync();

            var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
            var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);
            if (changePasswordResult.Success)
            {
                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Account.ChangePassword.Success"));
                return ApiResponseFactory.Success(model);
            }

            //errors
            var errors = changePasswordResult.Errors;
            if (errors.Any())
                return ApiResponseFactory.BadRequest(errors);

            //If we got this far, something failed, redisplay form
            return ApiResponseFactory.Success(model);
        }

        #endregion

        #region My account / Avatar

        /// <summary>
        ///     Prepare the customer avatar model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAvatarModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Avatar()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_customerSettings.AllowCustomersToUploadAvatars)} = false");

            var model = new CustomerAvatarModel();
            model = await _customerModelFactory.PrepareCustomerAvatarModelAsync(model);

            return ApiResponseFactory.Success(model.ToDto<CustomerAvatarModelDto>());
        }

        /// <summary>
        ///     Upload avatar
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAvatarModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UploadAvatar([FromBody] byte[] fileBinary,
            [FromQuery] [Required] string fileName,
            [FromQuery] [Required] string contentType)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_customerSettings.AllowCustomersToUploadAvatars)} = false");

            var customer = await _workContext.GetCurrentCustomerAsync();

            var customerAvatar = await _pictureService.GetPictureByIdAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute));
            if (fileBinary != null && !string.IsNullOrEmpty(fileName))
            {
                var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                if (fileBinary.Length > avatarMaxSize)
                    throw new NopException(string.Format(
                        await _localizationService.GetResourceAsync("Account.Avatar.MaximumUploadedFileSize"),
                        avatarMaxSize));

                var customerPictureBinary = fileBinary;
                if (customerAvatar != null)
                    customerAvatar = await _pictureService.UpdatePictureAsync(customerAvatar.Id, customerPictureBinary,
                        contentType, null);
                else
                    customerAvatar = await _pictureService.InsertPictureAsync(customerPictureBinary, contentType, null);
            }

            var customerAvatarId = 0f;
            if (customerAvatar != null)
                customerAvatarId = customerAvatar.Id;

            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute,
                customerAvatarId);

            var avatarUrl = await _pictureService.GetPictureUrlAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute),
                _mediaSettings.AvatarPictureSize,
                false);

            return ApiResponseFactory.Success(new CustomerAvatarModelDto { AvatarUrl = avatarUrl });
        }


        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CustomerAvatarModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UploadByFileAvatar([FromForm] IFormFile file)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_customerSettings.AllowCustomersToUploadAvatars)} = false");

            var customer = await _workContext.GetCurrentCustomerAsync();

            var customerAvatar = await _pictureService.GetPictureByIdAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute));


            if (file != null && !string.IsNullOrEmpty(file.Name))
            {
                byte[] customerPictureBinary = null;

                await using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    customerPictureBinary = ms.ToArray();
                }

                var avatarMaxSize = _customerSettings.AvatarMaximumSizeBytes;
                if (file.Length > avatarMaxSize)
                    throw new NopException(string.Format(
                        await _localizationService.GetResourceAsync("Account.Avatar.MaximumUploadedFileSize"),
                        avatarMaxSize));

                if (customerAvatar != null)
                {
                    await _pictureService.DeletePictureAsync(customerAvatar);
                }

                customerAvatar =
                    await _pictureService.InsertPictureAsync(customerPictureBinary, file.ContentType, null);
            }

            var customerAvatarId = 0;
            if (customerAvatar != null)
                customerAvatarId = customerAvatar.Id;

            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute,
                customerAvatarId);

            var avatarUrl = await _pictureService.GetPictureUrlAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute), _mediaSettings.AvatarPictureSize,
                false);

            return ApiResponseFactory.Success(new CustomerAvatarModelDto { AvatarUrl = avatarUrl });
        }

        /// <summary>
        ///     Remove avatar
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RemoveAvatar()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (!_customerSettings.AllowCustomersToUploadAvatars)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_customerSettings.AllowCustomersToUploadAvatars)} = false");

            var customer = await _workContext.GetCurrentCustomerAsync();

            var customerAvatar = await _pictureService.GetPictureByIdAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer,
                    NopCustomerDefaults.AvatarPictureIdAttribute));
            if (customerAvatar != null)
                await _pictureService.DeletePictureAsync(customerAvatar);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AvatarPictureIdAttribute,
                0);

            return ApiResponseFactory.Success();
        }

        #endregion

        #region GDPR tools

        /// <summary>
        ///     Prepare the GDPR tools model
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GdprToolsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GdprTools()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (!_gdprSettings.GdprEnabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_gdprSettings.GdprEnabled)} = false");

            var model = await _customerModelFactory.PrepareGdprToolsModelAsync();

            return ApiResponseFactory.Success(model.ToDto<GdprToolsModelDto>());
        }

        /// <summary>
        ///     Export customer info (GDPR request) to XLSX
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GdprToolsExport()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (!_gdprSettings.GdprEnabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_gdprSettings.GdprEnabled)} = false");

            //log
            await _gdprService.InsertLogAsync(await _workContext.GetCurrentCustomerAsync(), 0,
                GdprRequestType.ExportData, await _localizationService.GetResourceAsync("Gdpr.Exported"));

            //export
            var bytes = await _exportManager.ExportCustomerGdprInfoToXlsxAsync(
                await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);

            return File(bytes, MimeTypes.TextXlsx, "customerdata.xlsx");
        }

        /// <summary>
        ///     Gdpr tools delete
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GdprToolsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GdprToolsDelete()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest("Customer is not registered.");

            if (!_gdprSettings.GdprEnabled)
                return ApiResponseFactory.NotFound($"The setting {nameof(_gdprSettings.GdprEnabled)} = false");

            //log
            await _gdprService.InsertLogAsync(await _workContext.GetCurrentCustomerAsync(), 0,
                GdprRequestType.DeleteCustomer, await _localizationService.GetResourceAsync("Gdpr.DeleteRequested"));

            var model = await _customerModelFactory.PrepareGdprToolsModelAsync();
            model.Result = await _localizationService.GetResourceAsync("Gdpr.DeleteRequested.Success");

            return ApiResponseFactory.Success(model.ToDto<GdprToolsModelDto>());
        }

        #endregion

        #region Check gift card balance

        /// <summary>
        ///     Prepare the check gift card balance madel
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CheckGiftCardBalanceModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckGiftCardBalance()
        {
            if (!(_captchaSettings.Enabled && _customerSettings.AllowCustomersToCheckGiftCardBalance))
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_captchaSettings.Enabled)} and setting {nameof(_customerSettings.AllowCustomersToCheckGiftCardBalance)} is false");

            var model = await _customerModelFactory.PrepareCheckGiftCardBalanceModelAsync();

            return ApiResponseFactory.Success(model.ToDto<CheckGiftCardBalanceModelDto>());
        }

        /// <summary>
        ///     Check GiftCard balance
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CheckGiftCardBalanceModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CheckBalance([FromBody] CheckGiftCardBalanceModelDto model)
        {
            var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: model.GiftCardCode))
                .FirstOrDefault();
            if (giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard))
            {
                var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                    await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard),
                    await _workContext.GetWorkingCurrencyAsync());
                model.Result = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);
            }
            else
            {
                model.Message =
                    await _localizationService.GetResourceAsync("CheckGiftCardBalance.GiftCardCouponCode.Invalid");
            }

            return ApiResponseFactory.Success(model);
        }

        #endregion

        #endregion
    }
}
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Services.Currencies;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Themes;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class CommonController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public CommonController(
            CommonSettings commonSettings,
            ICommonModelFactory commonModelFactory,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings,
            StoreInformationSettings storeInformationSettings,
            VendorSettings vendorSettings,
            ICurrencyTools currencyTools,
            IRepository<Order> repository,
            CurrencyFactory currencyFactory)
        {
            _commonSettings = commonSettings;
            _commonModelFactory = commonModelFactory;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _sitemapSettings = sitemapSettings;
            _sitemapXmlSettings = sitemapXmlSettings;
            _storeInformationSettings = storeInformationSettings;
            _vendorSettings = vendorSettings;
            _currencyTools = currencyTools;
            _repository = repository;
            _currencyFactory = currencyFactory;
        }

        #endregion

        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly SitemapSettings _sitemapSettings;
        private readonly SitemapXmlSettings _sitemapXmlSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ICurrencyTools _currencyTools;
        private readonly IRepository<Order> _repository;
        private readonly CurrencyFactory _currencyFactory;

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetContextAsync()
        {
            var primaryCurrency = await _currencyTools.GetPrimaryCurrencyAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var selectedLanguage = await _workContext.GetWorkingLanguageAsync();
            var selectedCurrency = await _workContext.GetWorkingCurrencyAsync();
            
            var foo = new
            {
                CustomerId = customer.Id,
                VendorId = (await _workContext.GetCurrentVendorAsync())?.Id,
                CurrencyId = selectedCurrency.Id,
                PrimaryCurrencyId = primaryCurrency.Id,
                LanguageId = selectedLanguage.Id,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,

                PrimaryCurrency = await _currencyFactory.PrepareCurrencyAsync(primaryCurrency, selectedCurrency, selectedLanguage),
                Currency = await _currencyFactory.PrepareCurrencyAsync(selectedCurrency),
                Language = selectedLanguage
            };

            return ApiResponseFactory.Success(foo);
        }


        /// <summary>
        ///     Set language
        /// </summary>
        [HttpPost("{langId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SetLanguage(int langId, [FromQuery] [Required] string returnUrl)
        {
            var language = await _languageService.GetLanguageByIdAsync(langId);
            if (!language?.Published ?? false)
                language = await _workContext.GetWorkingLanguageAsync();

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            //language part in URL
            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                //remove current language code if it's already localized URL
                if ((await returnUrl.IsLocalizedUrlAsync(Request.PathBase, true)).IsLocalized)
                    returnUrl = returnUrl.RemoveLanguageSeoCodeFromUrl(Request.PathBase, true);

                //and add code of passed language
                returnUrl = returnUrl.AddLanguageSeoCodeToUrl(Request.PathBase, true, language);
            }

            await _workContext.SetWorkingLanguageAsync(language);

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            return ApiResponseFactory.Success(returnUrl);
        }

        /// <summary>
        ///     Set currency
        /// </summary>
        [HttpPost("{customerCurrencyId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SetCurrency(int customerCurrencyId,
            [FromQuery] [Required] string returnUrl)
        {
            var currency = await _currencyService.GetCurrencyByIdAsync(customerCurrencyId);
            if (currency != null)
                await _workContext.SetWorkingCurrencyAsync(currency);

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            return ApiResponseFactory.Success(returnUrl);
        }

        /// <summary>
        ///     Set tax type
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SetTaxType([FromQuery] [Required] TaxDisplayType customerTaxType,
            [FromQuery] [Required] string returnUrl)
        {
            await _workContext.SetTaxDisplayTypeAsync(customerTaxType);

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("Homepage");

            return ApiResponseFactory.Success(returnUrl);
        }

        /// <summary>
        ///     Contact us page
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ContactUsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ContactUs()
        {
            var model = new ContactUsModel();
            model = await _commonModelFactory.PrepareContactUsModelAsync(model, false);

            return ApiResponseFactory.Success(model.ToDto<ContactUsModelDto>());
        }

        /// <summary>
        ///     Contact us send
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ContactUsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ContactUsSend([FromBody] ContactUsModelDto model)
        {
            var contactUsModel =
                await _commonModelFactory.PrepareContactUsModelAsync(model.FromDto<ContactUsModel>(), true);

            var subject = _commonSettings.SubjectFieldOnContactUsForm ? contactUsModel.Subject : null;
            var body = HtmlHelper.FormatText(contactUsModel.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactUsMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                contactUsModel.Email.Trim(), contactUsModel.FullName, subject, body);

            contactUsModel.SuccessfullySent = true;
            contactUsModel.Result = await _localizationService.GetResourceAsync("ContactUs.YourEnquiryHasBeenSent");

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ContactUs",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ContactUs"));

            return ApiResponseFactory.Success(contactUsModel.ToDto<ContactUsModelDto>());
        }

        /// <summary>
        ///     contact vendor page
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        [HttpGet("{vendorId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ContactVendorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ContactVendor(int vendorId)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_vendorSettings.AllowCustomersToContactVendors)} is not enabled.");

            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return ApiResponseFactory.NotFound($"The manufacturer by id={vendorId} is not found.");

            var model = new ContactVendorModel();
            model = await _commonModelFactory.PrepareContactVendorModelAsync(model, vendor, false);

            return ApiResponseFactory.Success(model.ToDto<ContactVendorModelDto>());
        }

        /// <summary>
        ///     Contact vendor vend
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ContactVendorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ContactVendorSend([FromBody] ContactVendorModelDto model)
        {
            if (!_vendorSettings.AllowCustomersToContactVendors)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_vendorSettings.AllowCustomersToContactVendors)} is not enabled.");

            var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return ApiResponseFactory.NotFound($"The manufacturer by id={model.VendorId} is not found.");

            var contactVendorModel =
                await _commonModelFactory.PrepareContactVendorModelAsync(model.FromDto<ContactVendorModel>(), vendor,
                    true);

            var subject = _commonSettings.SubjectFieldOnContactUsForm ? contactVendorModel.Subject : null;
            var body = HtmlHelper.FormatText(contactVendorModel.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactVendorMessageAsync(vendor,
                (await _workContext.GetWorkingLanguageAsync()).Id,
                contactVendorModel.Email.Trim(), contactVendorModel.FullName, subject, body);

            contactVendorModel.SuccessfullySent = true;
            contactVendorModel.Result =
                await _localizationService.GetResourceAsync("ContactVendor.YourEnquiryHasBeenSent");

            return ApiResponseFactory.Success(contactVendorModel.ToDto<ContactVendorModelDto>());
        }

        /// <summary>
        ///     sitemap page
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SitemapModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Sitemap([FromBody] SitemapPageModelDto pageModel)
        {
            if (!_sitemapSettings.SitemapEnabled)
                return ApiResponseFactory.NotFound(
                    $"The setting {nameof(_sitemapSettings.SitemapEnabled)} is not enabled.");

            var model = await _commonModelFactory.PrepareSitemapModelAsync(pageModel.FromDto<SitemapPageModel>());

            return ApiResponseFactory.Success(model.ToDto<SitemapModelDto>());
        }

        /// <summary>
        ///     SEO sitemap page
        /// </summary>
        /// <param name="id">Sitemap identifier; pass 0 to load the first sitemap or sitemap index file</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SitemapXmlResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SitemapXml(int id)
        {
            var siteMap = _sitemapXmlSettings.SitemapXmlEnabled
                ? await _commonModelFactory.PrepareSitemapXmlAsync(id == 0 ? null : id)
                : string.Empty;

            var response = new SitemapXmlResponse
            {
                MimeType = "text/xml",
                SiteMapXML = siteMap
            };
            return ApiResponseFactory.Success(response);
        }

        /// <summary>
        ///     Set store theme
        /// </summary>
        /// <param name="themeName">Theme name</param>
        [HttpGet]
        public virtual async Task<IActionResult> SetStoreTheme([FromQuery] [Required] string themeName)
        {
            await _themeContext.SetWorkingThemeNameAsync(themeName);
            return ApiResponseFactory.Success();
        }

        /// <summary>
        ///     Eu cookie law accept
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> EuCookieLawAccept()
        {
            if (!_storeInformationSettings.DisplayEuCookieLawWarning)
                //disabled
                return ApiResponseFactory.Success(false);

            //save setting
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.EuCookieLawAcceptedAttribute, true,
                (await _storeContext.GetCurrentStoreAsync()).Id);
            return ApiResponseFactory.Success(true);
        }

        /// <summary>
        ///     robots.txt file
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(RobotsTextFileResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RobotsTextFile()
        {
            var robotsFileContent = await _commonModelFactory.PrepareRobotsTextFileAsync();
            var response = new RobotsTextFileResponse
            {
                RobotsFileContent = robotsFileContent,
                MimeType = MimeTypes.TextPlain
            };
            return ApiResponseFactory.Success(response);
        }

        [HttpGet]
        [Route("/PaymentMyFatoorah/Homepage")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> DoRedirect()
        {
            var order = await _repository.Table.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefaultAsync();
            
            return Redirect($"https://trydasman.com/orderdetails/{order?.Id ?? 0}");
        }

        #endregion
    }
}
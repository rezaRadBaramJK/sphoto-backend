using System;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Settings;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs
{
    public class PhotoPlatformContactUsNotifyService
    {
        private readonly IEmailSender _emailSender;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly PhotoPlatformContactUsMessageTemplateService _photoPlatformContactUsMessageTemplateService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly PhotoPlatformContactUsSettings _contactUsSettings;
        private readonly ICountryService _countryService;
        private readonly PhotoPlatformSubjectService _photoPlatformSubjectService;
        private readonly ILogger _logger;

        public PhotoPlatformContactUsNotifyService(
            IEmailSender emailSender,
            EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            PhotoPlatformContactUsMessageTemplateService photoPlatformContactUsMessageTemplateService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            PhotoPlatformContactUsSettings contactUsSettings,
            ICountryService countryService,
            PhotoPlatformSubjectService photoPlatformSubjectService,
            ILogger logger)
        {
            _emailSender = emailSender;
            _emailAccountSettings = emailAccountSettings;
            _emailAccountService = emailAccountService;
            _photoPlatformContactUsMessageTemplateService = photoPlatformContactUsMessageTemplateService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _contactUsSettings = contactUsSettings;
            _countryService = countryService;
            _photoPlatformSubjectService = photoPlatformSubjectService;
            _logger = logger;
        }


        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NopException"></exception>
        public async Task SendEmailAsync(string to, string subject, string message)
        {
            if (string.IsNullOrEmpty(to))
                throw new ArgumentNullException(nameof(to));

            if (_emailAccountSettings.DefaultEmailAccountId == 0)
                throw new NopException("Default email account does not configured.");

            var emailAccount =
                await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);

            if (emailAccount == null)
                throw new NopException("Default email account not found.");

            await _emailSender.SendEmailAsync(
                emailAccount,
                subject,
                message,
                emailAccount.Email,
                emailAccount.DisplayName,
                to,
                null);
        }


        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NopException"></exception>
        public async Task SendContactOwnerPaymentSuccessfulEmailAsync(ContactInfoEntity contactInfo)
        {
            if (string.IsNullOrEmpty(_contactUsSettings.NotifyAdminEmail))
            {
                await _logger.ErrorAsync("ContactUs: Contact owner payment successful error - admin email not found.");
                return;
            }

            var messageTemplate = await _photoPlatformContactUsMessageTemplateService.GetContactOwnerPaymentSuccessfulMessageTemplateAsync();
            if (messageTemplate == null)
            {
                await _logger.ErrorAsync("ContactUs: Contact owner payment successful error - message template not found.");
                return;
            }

            if (contactInfo == null)
            {
                await _logger.ErrorAsync($"ContactUs: Contact owner payment successful error - Invalid contact info.");
                return;
            }

            if (string.IsNullOrEmpty(contactInfo.Email))
            {
                await _logger.ErrorAsync(
                    $"ContactUs: Contact owner payment successful error - Invalid email for contact info with id {contactInfo.Id}.");
                return;
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            var storeName = await _localizationService.GetLocalizedAsync(store, s => s.Name);

            var subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject);
            subject = subject.Replace("%Store.Name%", storeName);
            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body);
            body = FormatString(body, new { customerInfo = await GenerateContactInfoString(contactInfo) });
            body = body.Replace("%Store.Name%", storeName);

            var to = _contactUsSettings.NotifyAdminEmail;
            await SendEmailAsync(to, subject, body);
        }

        private async Task<string> GenerateContactInfoString(ContactInfoEntity contactInfo)
        {
            var country = await _countryService.GetCountryByIdAsync(contactInfo.CountryId);
            var subject = await _photoPlatformSubjectService.GetByIdAsync(contactInfo.SubjectId);
            var hasPaid = contactInfo.HasBeenPaid ? "Yes" : "No";
            var paymentDate = contactInfo.PaymentUtcDateTime.HasValue
                ? contactInfo.PaymentUtcDateTime.Value.ToLocalTime().ToString("g")
                : string.Empty;
            return new StringBuilder()
                .Append($"<br><strong>Name:</strong>{contactInfo.FirstName} {contactInfo.LastName}")
                .Append($"<br><strong>Country:</strong>{country?.Name}")
                .Append($"<br><strong>Phone number:</strong>{contactInfo.PhoneNumber}")
                .Append($"<br><strong>Email:</strong>{contactInfo.Email}")
                .Append($"<br><strong>Subject:</strong>{subject.Name}")
                .Append($"<br><strong>Message:</strong>{contactInfo.Message}")
                .Append($"<br><strong>Has Paid:</strong>{hasPaid}")
                .Append($"<br><strong>Payment Date:</strong>{paymentDate}")
                .ToString();
        }

        private static string FormatString(string template, object parameters)
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
    }
}
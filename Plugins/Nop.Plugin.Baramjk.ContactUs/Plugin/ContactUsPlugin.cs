using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.ContactUs.Areas.Admin.Models.Settings;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.ContactUs.Plugin
{
    public class ContactUsPlugin: BaramjkPlugin, IAdminMenuPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public ContactUsPlugin(
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
        }
        
        public override Task InstallAsync()
        {
            return Task.WhenAll(
                _localizationService.AddLocaleResourceAsync(Localization),
                _permissionService.InstallPermissionsAsync(new PermissionProvider()));
        }

        public override Task UninstallAsync()
        {
            return
                Task.WhenAll(_localizationService.DeleteLocaleResourcesAsync("Baramjk.ContactUs"));
        }
        
        
        private static SiteMapNode _cachePluginSiteMapNode;
        
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await AuthorizeAsync(PermissionProvider.ManagementRecord) == false)
                return;

            if (_cachePluginSiteMapNode == null)
            {
                var subjects = CreateSiteMapNode(
                    "Subject",
                    "List",
                    "Subjects",
                    $"{SystemName}.Subjects.List");
                
                var contactInfos = CreateSiteMapNode(
                    "ContactInfo",
                    "List",
                    "Contact Infos",
                    $"{SystemName}.ContactInfos.List");
                
                _cachePluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, subjects, contactInfos);
            }

            rootNode.AddToBaramjkPluginsMenu(_cachePluginSiteMapNode);
        }
        
        public override string GetConfigurationPageUrl()
        {
            return $"{WebHelper.GetStoreLocation()}Admin/{Name}/Configure";
        }

        public static Dictionary<string, string> Localization => new()
        {
            //Plugin
            {"Baramjk.ContactUs.Admin.Settings.PaymentCallbackUrl" , "Payment Callback Url"},
            //subjects
            {"Baramjk.ContactUs.Admin.Subjects.Title" , "Subjects"},
            {"Baramjk.ContactUs.Admin.Subjects.AddSubject" , "Add New Subject"},
            {"Baramjk.ContactUs.Admin.Subjects.EditSubject" , "Edit Subject"},
            {"Baramjk.ContactUs.Admin.Subjects.Name" , "Name"},
            {"Baramjk.ContactUs.Admin.Subjects.BackToSubjectsList" , "Back to subjects list"},
            {"Baramjk.ContactUs.Admin.Subjects.IsPayable" , "Is Payable"},
            {"Baramjk.ContactUs.Admin.Subjects.Price" , "Price"},
            {"Baramjk.ContactUs.Admin.Subjects.NotifyAdminAfterPayment" , "Notify Admin After Payment"},
            {"Baramjk.ContactUs.Admin.Subjects.NotifyAdminEmail" , "Notify Admin Email"},
            {"Baramjk.ContactUs.Admin.Subjects.OwnerAdminEmail" , "Owner Admin Email"},
            {"Baramjk.ContactUs.Admin.Subjects.OwnerPhoneNumber" , "Owner Phone Number"},
            //Contact info
            {"Baramjk.ContactUs.Admin.ContactInfos.Title" , "Contact Infos"},
            {"Baramjk.ContactUs.Admin.ContactInfos.Edit" , "Edit Contact Info"},
            {"Baramjk.ContactUs.Admin.ContactInfos.BackToContactInfoList" , "Back T oContact Info List"},
            {"Baramjk.ContactUs.Admin.ContactInfos.FirstName" , "First Name"},
            {"Baramjk.ContactUs.Admin.ContactInfos.LastName" , "Last Name"},
            {"Baramjk.ContactUs.Admin.ContactInfos.FullName" , "Full Name"},
            {"Baramjk.ContactUs.Admin.ContactInfos.Country" , "Country"},
            {"Baramjk.ContactUs.Admin.ContactInfos.PhoneNumber" , "PhoneNumber"},
            {"Baramjk.ContactUs.Admin.ContactInfos.Email" , "Email"},
            {"Baramjk.ContactUs.Admin.ContactInfos.Subject" , "Subject"},
            {"Baramjk.ContactUs.Admin.ContactInfos.File" , "FileId"},
            {"Baramjk.ContactUs.Admin.ContactInfos.Message" , "Message"},
            {"Baramjk.ContactUs.Admin.ContactInfos.HasBeenPaid" , "Has Been Paid"},
            {"Baramjk.ContactUs.Admin.ContactInfos.SubjectPrice" , "Subject Price"},
            {"Baramjk.ContactUs.Admin.ContactInfos.PaymentDateTime" , "Payment Date Time"},
            {"Baramjk.ContactUs.Admin.ContactInfos.PaymentStatus" , "Payment Status"},
            {"Baramjk.ContactUs.Admin.ContactInfos.PaymentStatus.NoNeedToPay" , "No Need To Pay"},
            {"Baramjk.ContactUs.Admin.ContactInfos.PaymentStatus.Paid" , "Paid"},
            {"Baramjk.ContactUs.Admin.ContactInfos.PaymentStatus.HasNotPaid" , "Has not paid"},
        };

        public static ContactUsSettings DefaultSettings => new()
        {
            
        };
    }
}
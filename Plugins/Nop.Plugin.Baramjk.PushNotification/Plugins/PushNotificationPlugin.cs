using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.PushNotification.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.PushNotification.Plugins
{
    public class PushNotificationPlugin : BaramjkPlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly PushNotificationScheduleTaskService _pushNotificationScheduleTaskService;
        private readonly EventNotificationConfigService _eventNotificationConfigService;

        public PushNotificationPlugin(ISettingService settingService,
            ILocalizationService localizationService, 
            PushNotificationScheduleTaskService pushNotificationScheduleTaskService,
            EventNotificationConfigService eventNotificationConfigService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _pushNotificationScheduleTaskService = pushNotificationScheduleTaskService;
            _eventNotificationConfigService = eventNotificationConfigService;
        }

        public static PushNotificationSettings GetDefaultSetting
        {
            get
            {
                var currentSettings = EngineContext.Current.Resolve<PushNotificationSettings>();

                return new PushNotificationSettings
                {
                    ServerKey = string.IsNullOrEmpty(currentSettings.ServerKey) ? string.Empty : currentSettings.ServerKey,
                    FireBaseConfig = string.IsNullOrEmpty(currentSettings.FireBaseConfig) ? string.Empty : currentSettings.FireBaseConfig,
                    Strategy = currentSettings.Strategy == 0 ? 1 : currentSettings.Strategy,
                    PrivateKeyConfig = string.IsNullOrEmpty(currentSettings.PrivateKeyConfig) ? string.Empty : currentSettings.PrivateKeyConfig
                };

            }
        }
        public static SmsProviderSetting GetDefaultSmsSetting => new()
        {
            PromotionalPassword = "",
            PromotionalSource = "",
            IsPromotionalEnabled = false,
            IsTransactionalEnabled = false,
            PromotionalUrl = "",
            TransactionalPassword = "",
            TransactionalSource = "",
            TransactionalUrl = "",
            PromotionalUserName = "",
            TransactionalUserName = "",
            TransactionalMSISDN = "",
            PromotionalAccountId = 0,
            TransactionalAccountId = 0,
            PromotionalMSISDN = "",
        };
        public static Dictionary<string, string> GetLocalizationResources => new()
        {
            //Platform types
            { $"{DefaultValue.SystemName}.PlatformType.All", "All" },
            { $"{DefaultValue.SystemName}.PlatformType.Android", "Android" },
            { $"{DefaultValue.SystemName}.PlatformType.Ios", "Ios" },
            { $"{DefaultValue.SystemName}.PlatformType.Web", "Web" },
            //Notification types
            { $"{DefaultValue.SystemName}.NotificationType.Normal", "Normal" },
            { $"{DefaultValue.SystemName}.NotificationType.HttpLink", "Http link" },
            { $"{DefaultValue.SystemName}.NotificationType.DeepLink", "Deep link" },
            { $"{DefaultValue.SystemName}.NotificationType.Order", "Order" },
            { $"{DefaultValue.SystemName}.NotificationType.Product", "Product" },
            { $"{DefaultValue.SystemName}.NotificationType.Category", "Category" },
            { $"{DefaultValue.SystemName}.NotificationType.Congratulation", "Congratulation" },
            //Model fields
            { $"{DefaultValue.SystemName}.FireBaseConfig", "Firebase config" },
            { $"{DefaultValue.SystemName}.ServerKey", "Secret key" },
            { $"{DefaultValue.SystemName}.Picture", "Picture" },
            { $"{DefaultValue.SystemName}.Title", "Title" },
            { $"{DefaultValue.SystemName}.Body", "Body" },
            { $"{DefaultValue.SystemName}.Link", "Link" },
            { $"{DefaultValue.SystemName}.NotificationType", "Notificaion type" },
            { $"{DefaultValue.SystemName}.Code", "Code" },
            { $"{DefaultValue.SystemName}.ExtraData", "ExtraData" },
            { $"{DefaultValue.SystemName}.Platform", "Platform" },
            { $"{DefaultValue.SystemName}.Device", "Device" },
            { $"{DefaultValue.SystemName}.Token", "Token" },
            { $"{DefaultValue.SystemName}.PrivateKeyConfig", "Private Key Config" },
            { $"{DefaultValue.SystemName}.Strategy", "Strategy" },
            { $"{DefaultValue.SystemName}.FcmLegacy", "FCM Legacy" },
            { $"{DefaultValue.SystemName}.FcmHttpV1", "FCM HTTP v1" },
            { $"{DefaultValue.SystemName}.DisableNotification", "Disable Notification" },
            //View fileds
            { $"{DefaultValue.SystemName}.image", "Image" },
            { $"{DefaultValue.SystemName}.OnDatetime", "Sent time" },
            { $"{DefaultValue.SystemName}.NotificationList", "Notifications" },
            // sms fiedls
            { $"{DefaultValue.SystemName}.smslist", "Sms list" },
            { $"{DefaultValue.SystemName}.status", "Status" },
            { $"{DefaultValue.SystemName}.receptor", "Receptor" },
            { $"{DefaultValue.SystemName}.text", "Text" },
            { $"{DefaultValue.SystemName}.id", "id" },
            { $"{DefaultValue.SystemName}.Schedule", "Schedule" },
            { $"{DefaultValue.SystemName}.JobId", "JobId" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.TransactionalSource", "Transactional Source(Sender ID)" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.TransactionalMSISDN", "Transactional MSISDN" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.TransactionalAccountId", "Transactional Account Id" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.TransactionalUrl", "Transactional Url" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.TransactionalPassword", "Transactional Password" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.IsTransactionalEnabled", "Is Transactional Enabled" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.TransactionalUserName", "Transactional Username" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.IsPromotionalEnabled", "Is Promotional Enabled" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.PromotionalUrl", "Promotional Url" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.PromotionalSource", "Promotional Source(Sender ID)" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.PromotionalMSISDN", "Promotional MSISDN" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.PromotionalUserName", "Promotional Username" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.PromotionalPassword", "Promotional Password" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.PromotionalAccountId", "Promotional Account Id" },
            { $"{DefaultValue.SystemName}.Configuration.SMS.Provider", "Provider" },

            //what's app
            { $"{DefaultValue.SystemName}.whatsApp", "What's App" },
            { $"{DefaultValue.SystemName}.Configuration.WhatsAppSettings", "What's App settings" },
            { $"{DefaultValue.SystemName}.Configuration.WhatsApp.Username", "Username" },
            { $"{DefaultValue.SystemName}.Configuration.WhatsApp.Password", "Password" },
            { $"{DefaultValue.SystemName}.Configuration.WhatsApp.Provider", "Provider" },
            { $"{DefaultValue.SystemName}.Configuration.WhatsApp.ApiSid", "Api Sid" },
            { $"{DefaultValue.SystemName}.Configuration.WhatsApp.ApiSecret", "Api Secret" },
            { $"{DefaultValue.SystemName}.Configuration.WhatsApp.SenderPhoneNumber", "Sender Phone Number" },
            { $"{DefaultValue.SystemName}.Configuration.WhatsApp.OtpTemplateName", "Otp Template Name" },
            //EventNotificationConfig
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.Title", "Event Notification Configs" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.EventName", "Event Name" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.StatusName", "Status Name" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.TemplateName", "Template Name" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.UseSms", "Use Sms" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.UseFirebase", "Use Firebase" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.UseWhatsApp", "Use WhatsApp" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.AddNew", "Add New Event Notification Config" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.BackToList", "Back To List" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.EditDetails", "Edit Event Notification Config" },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.AlreadyExists", "This event notification config already exists." },
            { $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.OrderNoteInserted", "Order Note Inserted" },
        };

        protected static SiteMapNode PluginSiteMapNode;


        
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!(await AuthorizeAsync(PermissionProvider.Management)))
                return;

            if (PluginSiteMapNode == null)
            {
                var smsManual = CreateSiteMapNode("SmsAdmin", "SendManualSms", "Send Manual Sms",$"{MenuUtils.BaramjkMenuSystemName}_PushNotification_ManualSms");
                var listSms = CreateSiteMapNode("SmsList", "GetSmsList", "List Sms");
                var smsTemplate = CreateSiteMapNode("SmsTemplateAdmin", "GetSmsTemplateConfiguration", "Sms Template",$"{MenuUtils.BaramjkMenuSystemName}_PushNotification_SmsTemplate");

                var send = CreateSiteMapNode("SendNotification", "SendNotification", "Send Notification");
                var list = CreateSiteMapNode("SendNotification", "List", "List Notification");
                var listSchedule = CreateSiteMapNode("ScheduleList", "ScheduleList", "List Schedule");

                PluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, smsManual,listSms,smsTemplate,send, list,listSchedule);
                var eventNotificationConfig = CreateSiteMapNode(
                    "EventNotificationConfig",
                    "Events",
                    await _localizationService.GetResourceAsync(
                        $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.Title"),
                    $"{SystemName}.EventNotificationConfig");
                PluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, smsManual,listSms,smsTemplate,send, list, eventNotificationConfig);
            }

            rootNode.AddToBaramjkPluginsMenu(PluginSiteMapNode);
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(GetDefaultSetting);
            await _settingService.SaveSettingAsync(GetDefaultSmsSetting);
            await _localizationService.AddLocaleResourceAsync(GetLocalizationResources);
            await PermissionService.InstallPermissionsAsync(new PermissionProvider());
            await _pushNotificationScheduleTaskService.AddAsync();
            await _eventNotificationConfigService.InitAsync();
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<PushNotificationSettings>();
            await _localizationService.DeleteLocaleResourcesAsync(GetLocalizationResources.Keys.ToList());
            await base.UninstallAsync();
        }

        public bool HideInWidgetList => false;

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.HomepageBottom
            });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "PushNotificationComponent";
        }
    }
}
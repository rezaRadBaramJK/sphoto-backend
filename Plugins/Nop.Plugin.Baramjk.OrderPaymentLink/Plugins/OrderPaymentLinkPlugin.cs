using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Nop.Plugins;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Plugin.Baramjk.OrderPaymentLink.ImplementNopPlugin;
using Nop.Plugin.Baramjk.OrderPaymentLink.Services.Model;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Baramjk.OrderPaymentLink.Plugins
{
    public class OrderPaymentLinkPlugin : BaramjkPlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public OrderPaymentLinkPlugin(IMessageTemplateService messageTemplateService,
            IPermissionService permissionService, IRepository<PermissionRecord> permissionRecordRepository,
            ISettingService settingService, ILocalizationService localizationService)
        {
            _messageTemplateService = messageTemplateService;
            _permissionService = permissionService;
            _permissionRecordRepository = permissionRecordRepository;
            _settingService = settingService;
            _localizationService = localizationService;
        }

        private static SiteMapNode _cachePluginSiteMapNode;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!(await AuthorizeAsync(PermissionProvider.Management)))
                return;

            if (_cachePluginSiteMapNode == null)
            {
                var node = CreateSiteMapNode("OrderPaymentLinkReport", "Report", "Report");
                _cachePluginSiteMapNode = CreatePluginSiteMapNode(FriendlyName, nodes: node);
            }

            rootNode.AddToBaramjkPluginsMenu(_cachePluginSiteMapNode);
        }

        public override async Task InstallAsync()
        {
            var messageTemplate = new MessageTemplate
            {
                Name = "OrderPaymentInvoiceLink",
                Body = "Order Payment Invoice <br/><a href=\"%InvoiceLink%\">%InvoiceLink%</a> .",
                Subject = "Order Payment Invoice",
                IsActive = true
            };
            await _messageTemplateService.InsertMessageTemplateAsync(messageTemplate);

            await _permissionRecordRepository.InsertAsync(PermissionRecords.CreateOrderPaymentLink);

            await _settingService.SaveSettingAsync(new OrderPaymentLinkSetting());

            await _localizationService.AddLocaleResourceAsync(Localization);
            
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            var createOrderPaymentLink = _permissionRecordRepository.Table
                .FirstOrDefault(item => item.SystemName == PermissionRecords.CreateOrderPaymentLink.SystemName);

            var reportOrderPaymentLink = _permissionRecordRepository.Table
                .FirstOrDefault(item => item.SystemName == PermissionRecords.ReportOrderPaymentLink.SystemName);

            if (createOrderPaymentLink != null)
                await _permissionRecordRepository.InsertAsync(createOrderPaymentLink);

            if (reportOrderPaymentLink != null)
                await _permissionRecordRepository.InsertAsync(reportOrderPaymentLink);


            await _settingService.DeleteSettingAsync<OrderPaymentLinkSetting>();
            await _localizationService.DeleteLocaleResourcesAsync("Baramjk.OrderPaymentLink");
            await base.UninstallAsync();
        }

        public bool HideInWidgetList => false;

        public async Task<IList<string>> GetWidgetZonesAsync()
        {
            return new List<string>
            {
                AdminWidgetZones.OrderDetailsBlock,
                PublicWidgetZones.CheckoutCompletedBottom
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsOrderPaymentLink";
        }

        public static readonly Dictionary<string, string> Localization = new()
        {
            { "Nop.Plugin.Baramjk.OrderPaymentLink.Admin.Configuration.Message", "Message" },
            { "Nop.Plugin.Baramjk.OrderPaymentLink.Admin.Configuration.LinkExpireAfterMinutes", "Link Expiration In Minutes" },
        };
    }
}
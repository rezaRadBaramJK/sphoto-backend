using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Baramjk.Framework.Events;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Models.Admin.EventNotificationConfigs;
using Nop.Plugin.Baramjk.PushNotification.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PushNotification.Factories.Admin
{
    public class EventNotificationConfigFactory
    {
        private readonly EventNotificationConfigService _eventNotificationConfigService;
        private readonly ILocalizationService _localizationService;

        public EventNotificationConfigFactory(
            EventNotificationConfigService eventNotificationConfigService,
            ILocalizationService localizationService)
        {
            _eventNotificationConfigService = eventNotificationConfigService;
            _localizationService = localizationService;
        }

        public async Task<EventNotificationConfigListModel> EventNotificationConfigListModelAsync(
            EventNotificationConfigSearchModel searchModel)
        {
            var pageIndex = searchModel.Page - 1;
            var entities = await _eventNotificationConfigService.GetAllAsync(
                pageIndex,
                searchModel.PageSize);
            
            var listModel = await new EventNotificationConfigListModel().PrepareToGridAsync(searchModel, entities,
                () =>
                {
                    return entities.SelectAwait(async e =>
                    {
                        var eventNameResourceKey = GetResourceKeyByEventKey(e.EventName);
                        var withoutStatusName = string.IsNullOrEmpty(e.StatusName);
                        var localizedStatusName = "--";
                        if (withoutStatusName == false)
                        {
                            var enumType = GetEnumType(e.EventName);
                            var statusResourceKey = $"{NopLocalizationDefaults.EnumLocaleStringResourcesPrefix}{enumType}.{e.StatusName}";
                            localizedStatusName = await _localizationService.GetResourceAsync(statusResourceKey);
                        }
                        
                        return new EventNotificationConfigModel
                        {
                            Id = e.Id,
                            EventName = e.EventName,
                            LocalizedEventName = await _localizationService.GetResourceAsync(eventNameResourceKey),
                            StatusName = e.StatusName,
                            LocalizedStatusName = localizedStatusName,
                            TemplateName = e.TemplateName,
                            UseFirebase = e.UseFirebase,
                            UseSms = e.UseSms,
                            UseWhatsApp = e.UseWhatsApp
                        };
                    });
                });

            listModel.Data = listModel.Data
                .OrderBy(m => m.EventName)
                .ThenBy(m => m.StatusName);

            return listModel;
        }

        public async Task<EventNotificationConfigModel> PrepareModelAsync(EventNotificationConfigEntity entity = null)
        {
            var availableEventNames = await PrepareAvailableEventNamesAsync(entity);

            var selectedEventName = string.Empty;
            var selectedEventListItem = availableEventNames.FirstOrDefault(s => s.Selected);
            
            if (selectedEventListItem != null)
                selectedEventName = selectedEventListItem.Value;
            
            var model = new EventNotificationConfigModel
            {
                EventName = selectedEventName,
                AvailableEventNames = availableEventNames,
            };

            if (entity == null)
                return model;
            
            model.Id = entity.Id;
            model.EventName = entity.EventName;
            model.StatusName = entity.StatusName;
            model.UseFirebase = entity.UseFirebase;
            model.TemplateName = entity.TemplateName;
            model.UseSms = entity.UseSms;
            model.UseWhatsApp = entity.UseWhatsApp;

            return model;
        }

        private async Task<IList<SelectListItem>> PrepareAvailableEventNamesAsync(EventNotificationConfigEntity entity)
        {
            var availableEvents = new List<SelectListItem>
            {
                await GetEventNameSelectListItemAsync(EventKeys.OrderOrderStatusKey, entity),
                await GetEventNameSelectListItemAsync(EventKeys.OrderPaymentStatusKey, entity),
                await GetEventNameSelectListItemAsync(EventKeys.OrderShippingStatusKey, entity),
                new()
                {
                    Text = await _localizationService.GetResourceAsync($"{DefaultValue.SystemName}.Admin.EventNotificationConfig.OrderNoteInserted"),
                    Value = DefaultValue.OrderNoteInsertedEventKey,
                    Selected = entity is { EventName: DefaultValue.OrderNoteInsertedEventKey }
                },
            };

            if (availableEvents.Count > 0 && (availableEvents.Any(n => n.Selected) == false))
                availableEvents.First().Selected = true;

            return availableEvents;
        }

        private async Task<SelectListItem> GetEventNameSelectListItemAsync(string eventKey, EventNotificationConfigEntity entity)
        {
            var resourceKey = GetResourceKeyByEventKey(eventKey);
            return new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync(resourceKey),
                Value = eventKey,
                Selected = entity != null && entity.EventName == eventKey
            };
        }

        private static string GetResourceKeyByEventKey(string eventKey)
        {
            return eventKey switch
            {
                EventKeys.OrderOrderStatusKey => "Admin.Orders.Fields.OrderStatus",
                EventKeys.OrderPaymentStatusKey => "Admin.Orders.Fields.PaymentStatus",
                EventKeys.OrderShippingStatusKey => "Admin.Orders.Fields.ShippingStatus",
                DefaultValue.OrderNoteInsertedEventKey => $"{DefaultValue.SystemName}.Admin.EventNotificationConfig.OrderNoteInserted",
                _ => string.Empty
            };
        }
        
        private static Type GetEnumType(string eventKey)
        {
            return eventKey switch
            {
                EventKeys.OrderOrderStatusKey => typeof(OrderStatus),
                EventKeys.OrderPaymentStatusKey => typeof(PaymentStatus),
                EventKeys.OrderShippingStatusKey => typeof(ShippingStatus),
                _ => throw new ArgumentOutOfRangeException(nameof(eventKey), eventKey, null)
            };
        }
        
        public EventNotificationConfigSearchModel PrepareSearchModel()
        {
            var model = new EventNotificationConfigSearchModel();
            model.SetGridPageSize();
            return model;
        }

        public async Task<IList<SelectListItem>> PrepareStatusNameListItemsByEventNameAsync(string eventName, int configId)
        {
            if (string.IsNullOrEmpty(eventName))
                return new List<SelectListItem>();

            var config = await _eventNotificationConfigService.GetByIdAsync(configId);
            var selectedStatus = string.Empty;
            if (config != null)
                selectedStatus = config.StatusName;
            
            
            if (EventKeys.OrderOrderStatusKey == eventName)
                return await PrepareStatusNameListItemsAsync<OrderStatus>(selectedStatus);

            if (EventKeys.OrderPaymentStatusKey == eventName)
                return await PrepareStatusNameListItemsAsync<PaymentStatus>(selectedStatus);
            
            if (EventKeys.OrderShippingStatusKey == eventName)
                return await PrepareStatusNameListItemsAsync<ShippingStatus>(selectedStatus);
            
            return new List<SelectListItem>();

        }

        private async Task<IList<SelectListItem>> PrepareStatusNameListItemsAsync<TEnum>(string selectedStatus) where TEnum : struct, Enum
        {
            var statusNames = await Enum.GetValues<TEnum>()
                .SelectAwait(async e => new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync(e),
                    Value = e.ToString(),
                    Selected = e.ToString() == selectedStatus
                }).ToListAsync();

            if (statusNames.Count > 0 && (statusNames.Any(s => s.Selected) == false))
                statusNames.First().Selected = true;

            return statusNames;
        }

    }
}
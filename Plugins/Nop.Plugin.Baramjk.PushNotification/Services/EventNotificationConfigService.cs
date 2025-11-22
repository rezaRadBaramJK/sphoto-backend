using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Events;
using Nop.Plugin.Baramjk.PushNotification.Domain;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class EventNotificationConfigService
    {

        private readonly IRepository<EventNotificationConfigEntity> _eventNotificationConfigRepository;

        public EventNotificationConfigService(IRepository<EventNotificationConfigEntity> eventNotificationConfigRepository)
        {
            _eventNotificationConfigRepository = eventNotificationConfigRepository;
        }

        public Task InsertAsync(EventNotificationConfigEntity entity)
        {
            return _eventNotificationConfigRepository.InsertAsync(entity);
        }

        public async Task InsertIfNotExistsAsync(EventNotificationConfigEntity entityToInsert)
        {
            var existEntity = await GetAsync(entityToInsert.EventName, entityToInsert.StatusName);
            if (existEntity == null)
                await InsertAsync(entityToInsert);
        }

        public Task<IPagedList<EventNotificationConfigEntity>> GetAllAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            return _eventNotificationConfigRepository
                .GetAllPagedAsync(query => query, pageIndex, pageSize);
        }

        public Task<EventNotificationConfigEntity> GetAsync(
            string eventName,
            string statusName)
        {
            return _eventNotificationConfigRepository.Table.FirstOrDefaultAsync(enc =>
                enc.EventName == eventName &&
                enc.StatusName == statusName);
        }

        public Task<EventNotificationConfigEntity> GetByIdAsync(int id)
        {
            return _eventNotificationConfigRepository.GetByIdAsync(id);
        }

        public Task UpdateAsync(EventNotificationConfigEntity entity)
        {
            return _eventNotificationConfigRepository.UpdateAsync(entity);
        }

        public Task DeleteAsync(EventNotificationConfigEntity entity)
        {
            return _eventNotificationConfigRepository.DeleteAsync(entity);
        }
        
        public Task InitAsync()
        {
            return Task.WhenAll(
                InitOrderStatusAsync(),
                InitPaymentStatusAsync(),
                InitShippingStatusAsync(),
                InitOrderNoteStatusAsync());
        }

        private Task InitOrderStatusAsync()
        {
            var tasks = 
                Enum.GetValues<OrderStatus>()
                .Select(os => InsertIfNotExistsAsync(new EventNotificationConfigEntity
                {
                    EventName = EventKeys.OrderOrderStatusKey,
                    StatusName = os.ToString()
                }));

            return Task.WhenAll(tasks);
        }
        
        private Task InitPaymentStatusAsync()
        {
            var tasks = 
                Enum.GetValues<PaymentStatus>()
                    .Select(ps => InsertIfNotExistsAsync(new EventNotificationConfigEntity
                    {
                        EventName = EventKeys.OrderPaymentStatusKey,
                        StatusName = ps.ToString()
                    }));

            return Task.WhenAll(tasks);
        }
        
        private Task InitShippingStatusAsync()
        {
            var tasks = 
                Enum.GetValues<ShippingStatus>()
                    .Select(ss => InsertIfNotExistsAsync(new EventNotificationConfigEntity
                    {
                        EventName = EventKeys.OrderShippingStatusKey,
                        StatusName = ss.ToString()
                    }));

            return Task.WhenAll(tasks);
        }

        private Task InitOrderNoteStatusAsync()
        {
            return InsertIfNotExistsAsync(new EventNotificationConfigEntity
            {
                EventName = DefaultValue.OrderNoteInsertedEventKey,
                StatusName = string.Empty
            });
        }
        
    }
}
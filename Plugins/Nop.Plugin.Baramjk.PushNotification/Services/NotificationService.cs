using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PushNotification.Domain;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class NotificationService
    {
        private readonly IPushNotificationTokenService _pushNotificationTokenService;
        private readonly IRepository<PushNotificationItem> _repositoryNotificationItem;
        private readonly IRepository<PushNotificationItemRelCustomer> _repositoryNotificationItemRelCustomer;
        private readonly IRepository<Customer> _repositoryCustomer;

        public NotificationService(IRepository<PushNotificationItem> repositoryNotificationItem,
            IPushNotificationTokenService pushNotificationTokenService,
            IRepository<PushNotificationItemRelCustomer> repositoryNotificationItemRelCustomer,
            IRepository<Customer> repositoryCustomer)
        {
            _repositoryNotificationItem = repositoryNotificationItem;
            _pushNotificationTokenService = pushNotificationTokenService;
            _repositoryNotificationItemRelCustomer = repositoryNotificationItemRelCustomer;
            _repositoryCustomer = repositoryCustomer;
        }

        public async Task<List<NotificationDto>> GetNotificationAsync(int? customerId,
            NotificationPlatform? platform = null, bool? isRead = null, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            var query = from r in _repositoryNotificationItemRelCustomer.Table
                join n in _repositoryNotificationItem.Table on r.PushNotificationItemId equals n.Id
                join c in _repositoryCustomer.Table on r.CustomerId equals c.Id
                where
                    (customerId == null || r.CustomerId == customerId) &&
                    (platform == null || n.NotificationPlatform == platform) &&
                    (isRead == null || r.IsRead == isRead)
                select new NotificationDto
                {
                    Email = c.Email,
                    IsRead = r.IsRead,
                    Id = n.Id,
                    CustomerNotificationId = r.Id,
                    Title = n.Title,
                    Body = n.Body,
                    Image = n.Image,
                    Link = n.Link,
                    Code = n.Code,
                    Data = n.Data,
                    ExtraData = n.ExtraData,
                    NotificationPlatform = n.NotificationPlatform,
                    NotificationScopeType = n.NotificationScopeType,
                    NotificationType = n.NotificationType,
                    OnDateTime = n.OnDateTime
                };

            var notificationDtos = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            return notificationDtos;
        }

        public async Task<PushNotificationItemRelCustomer> MarkAsRead(int customerId, int id)
        {
            var item = await _repositoryNotificationItemRelCustomer.Table.FirstOrDefaultAsync(i =>
                i.Id == id && i.CustomerId == customerId);

            if (item == null)
                throw NotFoundBusinessException.NotFound("Not found item");

            item.IsRead = true;
            await _repositoryNotificationItemRelCustomer.UpdateAsync(item);

            return item;
        }

        public async Task<bool> HasUnReadNotificationAsync(int customerId)
        {
            return  await _repositoryNotificationItemRelCustomer
                .Table
                .AnyAsync(item => item.CustomerId == customerId && item.IsRead == false);
        }

        public async Task SaveNotificationsAsync(Notification notification)
        {
            var pushNotificationItem = await SavePushNotificationItem(notification);
            await SavePushNotificationItemRelCustomer(notification, pushNotificationItem.Id);
        }

        private async Task<PushNotificationItem> SavePushNotificationItem(Notification notification)
        {
            var pushNotificationItem = new PushNotificationItem
            {
                Title = notification.Title,
                Body = notification.Body,
                Image = notification.Image,
                Data = notification.Data != null ? JsonConvert.SerializeObject(notification.Data) : "",
                OnDateTime = DateTime.Now,
                NotificationType = notification.NotificationType,
                NotificationPlatform = notification.NotificationPlatform
            };

            if (notification.Data is PushNotificationData data)
            {
                pushNotificationItem.Code = data.Code;
                pushNotificationItem.Link = data.Link;
                pushNotificationItem.ExtraData = data.ExtraData;
                pushNotificationItem.NotificationScopeType = data.NotificationScopeType;
            }

            await _repositoryNotificationItem.InsertAsync(pushNotificationItem);
            return pushNotificationItem;
        }

        private async Task SavePushNotificationItemRelCustomer(Notification notification, int pushNotificationItemId)
        {
            var splitList = SplitList(notification.RegistrationIds.ToList(), 1000);
            foreach (var smallTokenList in splitList)
            {
                var tokens = await _pushNotificationTokenService.GetPushNotificationTokensAsync(smallTokenList);
                var itemRelCustomers = tokens.Select(i => new PushNotificationItemRelCustomer
                {
                    Token = i.Token,
                    CustomerId = i.CustomerId,
                    IsRead = false,
                    PushNotificationItemId = pushNotificationItemId
                }).ToList();

                await _repositoryNotificationItemRelCustomer.InsertAsync(itemRelCustomers);
            }
        }

        private static IEnumerable<List<string>> SplitList(List<string> bigList, int nSize = 3)
        {
            for (var i = 0; i < bigList.Count; i += nSize)
                yield return bigList.GetRange(i, Math.Min(nSize, bigList.Count - i));
        }
    }

    public class NotificationDto
    {
        public string Email { get; set; }
        public bool IsRead { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        public string NotificationType { get; set; }
        public string Link { get; set; }
        public string Code { get; set; }
        public string Data { get; set; }
        public string ExtraData { get; set; }
        public int CustomerNotificationId { get; set; }
        public NotificationScopeType NotificationScopeType { get; set; }
        public DateTime OnDateTime { get; set; }
        public NotificationPlatform? NotificationPlatform { get; set; }
    }
}
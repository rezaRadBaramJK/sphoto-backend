using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.BackgroundJobs;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PushNotification.Domain;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Services.Logging;
using Notification = Nop.Plugin.Baramjk.Framework.Services.PushNotifications.Notification;

namespace Nop.Plugin.Baramjk.PushNotification.Services
{
    public class Version1FcmPushNotificationSenderService : IPushNotificationSenderService
    {
        private static FirebaseApp _firebaseApp;
        private static string _lastPrivateKeyConfig;

        private readonly IEventPublisher _eventPublisher;
        private readonly PushNotificationSettings _pushNotificationSettings;
        private readonly IBaramjkBackgroundJob _baramjkBackgroundJob;
        private readonly ILogger _logger;
        private readonly IRepository<PushNotificationToken> _pushNotificationTokenRepository;
        private readonly ScheduleNotificationService _scheduleNotificationService;

        public Version1FcmPushNotificationSenderService(
            PushNotificationSettings pushNotificationSettings,
            IEventPublisher eventPublisher,
            ILogger logger,
            IRepository<PushNotificationToken> pushNotificationTokenRepository,
            ScheduleNotificationService scheduleNotificationService)
        {
            _pushNotificationSettings = pushNotificationSettings;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _pushNotificationTokenRepository = pushNotificationTokenRepository;

            //!Fayyaz: do not inject by constructor
            _baramjkBackgroundJob = EngineContext.Current.Resolve<IBaramjkBackgroundJob>();
            _scheduleNotificationService = scheduleNotificationService;
            Init();
        }

        private void Init()
        {
            if (string.IsNullOrEmpty(_pushNotificationSettings.PrivateKeyConfig))
            {
                _firebaseApp = null;
                _lastPrivateKeyConfig = string.Empty;
                return;
            }

            if (_firebaseApp != null && _lastPrivateKeyConfig.Equals(_pushNotificationSettings.PrivateKeyConfig))
                return;

            try
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(_pushNotificationSettings.PrivateKeyConfig)
                });
                _lastPrivateKeyConfig = _pushNotificationSettings.PrivateKeyConfig;
            }
            catch (Exception e)
            {
                _logger.InsertLogAsync(LogLevel.Error, "Version1FcmPushNotificationSenderService init error. please see more details.", e.StackTrace);
            }
        }

        public async Task SendAsync(Notification notification)
        {
            if (_pushNotificationSettings.DisableNotification)
            {
                await _logger.WarningAsync("Push notification is disabled.");
                return;
            }

            if (_firebaseApp == null)
                throw new NopException("firebase is invalid.");

            //! important: according to firebase admin sdk source code count of multicast messages should be equal less than 500 for every time call
            var splitTokens = SplitList(notification.RegistrationIds.ToList(), 500);
            var link = string.Empty;
            var code = string.Empty;
            if (notification.Data is PushNotificationData data)
            {
                link = data.Link;
                code = data.Code;
            }

            foreach (var currentTokens in splitTokens)
            {
                var multicastMessage = new MulticastMessage
                {
                    Tokens = currentTokens,
                    Data = new Dictionary<string, string>
                    {
                        { nameof(notification.NotificationType), notification.NotificationType },
                        { "Link", link },
                        { "Code", code },
                    },
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = notification.Title,
                        Body = notification.Body,
                        ImageUrl = string.IsNullOrEmpty(notification.Image) ? null : notification.Image,
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = string.IsNullOrEmpty(_pushNotificationSettings.SoundFileName) ? "default" : _pushNotificationSettings.SoundFileName
                        }
                    }
                };
                var result = await FirebaseMessaging.GetMessaging(_firebaseApp).SendEachForMulticastAsync(multicastMessage);
                if (result.SuccessCount != 1 && result.FailureCount != 0)
                {
                    var errorFullMessage = string.Join(" ########## \n", result.Responses
                        .Where(r => r.IsSuccess == false && r.Exception != null)
                        .Select(r => $"{r.Exception.MessagingErrorCode}: {r.Exception.Message}"));
                    
                    await _logger.InsertLogAsync(LogLevel.Error, 
                        $"Failed to send notification. Success: {result.SuccessCount}, Failure: {result.FailureCount}, Check more log for details.",
                        errorFullMessage);
                    
                }

                await DeleteExpiredTokenAsync(result, currentTokens);
            }

            await _eventPublisher.PublishAsync(notification);
        }

        private async Task DeleteExpiredTokenAsync(BatchResponse result, List<string> currentTokens)
        {
            var tokensToDelete = new List<string>();
            for (int i = 0; i < result.Responses.Count; i++)
            {
                if (!result.Responses[i].IsSuccess)
                {
                    var error = result.Responses[i].Exception.MessagingErrorCode;
                    if (error == MessagingErrorCode.Unregistered)
                    {
                        tokensToDelete.Add(currentTokens[i]);
                    }
                }
            }

            if (tokensToDelete.Any())
            {
                await _pushNotificationTokenRepository.DeleteAsync(x => tokensToDelete.Contains(x.Token));
            }
        }


        public Task SendAsync(IList<Notification> notifications)
        {
            if (_pushNotificationSettings.DisableNotification)
            {
                _logger.WarningAsync("Push notification is disabled.");
                return Task.CompletedTask;
            }

            if (_firebaseApp == null)
                throw new NopException("firebase is invalid.");

            if (notifications.Any() == false)
                return Task.CompletedTask;

            var tasks = notifications.Select(SendAsync).ToList();
            return Task.WhenAll(tasks);
        }

        public async Task SendAsync(Notification notification, DateTime dateTime)
        {
            if (_baramjkBackgroundJob == null)
            {
                await _logger.ErrorAsync("Background jon service not injected");
                return;
            }

            var jobId = _baramjkBackgroundJob.Schedule(() => SendAsync(notification),
                dateTime);
            await _scheduleNotificationService.InsertAsync(new ScheduleNotificationModel
            {
                JobId = jobId,
                OnDateTime = dateTime,
                Title = notification.Title
            });
        }

        private static IEnumerable<List<string>> SplitList(List<string> bigList, int nSize = 3)
        {
            for (var i = 0; i < bigList.Count; i += nSize)
                yield return bigList.GetRange(i, Math.Min(nSize, bigList.Count - i));
        }
    }
}
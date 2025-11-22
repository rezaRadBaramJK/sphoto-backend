using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Services.BackgroundJobs;
using Nop.Plugin.Baramjk.Framework.Services.PushNotifications;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Plugin.Baramjk.PushNotification.Services;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    //[License(PluginName = DefaultValue.SystemName, PageType = PageType.Api)]
    public class PushNotificationApiController : BaseBaramjkApiController
    {
        private readonly NotificationService _notificationService;
        private readonly IPictureService _pictureService;
        private readonly IPushNotificationSenderService _pushNotificationService;
        private readonly PushNotificationSettings _pushNotificationSettings;
        private readonly IPushNotificationTokenService _pushNotificationTokenService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;

        private readonly IBaramjkBackgroundJob _baramjkBackgroundJob;
        private readonly IRepository<ScheduleNotificationModel> _repository;
        public PushNotificationApiController(NotificationService notificationService,
            IPictureService pictureService, IPushNotificationSenderService pushNotificationService,
            PushNotificationSettings pushNotificationSettings,
            IPushNotificationTokenService pushNotificationTokenService, IWorkContext workContext, ILogger logger, IRepository<ScheduleNotificationModel> repository)
        {
            _notificationService = notificationService;
            _pictureService = pictureService;
            _pushNotificationService = pushNotificationService;
            _pushNotificationSettings = pushNotificationSettings;
            _pushNotificationTokenService = pushNotificationTokenService;
            _workContext = workContext;
            _logger = logger;
            //!Fayyaz: do not inject by constructor
            _baramjkBackgroundJob = EngineContext.Current.Resolve<IBaramjkBackgroundJob>();
            _repository = repository;
        }

        [HttpGet("/api-frontend/PushNotification/FirebaseConfig")]
        [HttpGet("/FrontendApi/PushNotification/FirebaseConfig")]
        [HttpGet("/PushNotification/FirebaseConfig")]
        public async Task<IActionResult> FirebaseConfig()
        {
            var settingsFireBaseConfig = _pushNotificationSettings.FireBaseConfig;
            return ApiResponseFactory.Success(settingsFireBaseConfig, "Ok");
        }

        [HttpGet("/api-frontend/PushNotification/userNotification/List")]
        [HttpGet("/FrontendApi/PushNotification/userNotification/List")]
        public async Task<IActionResult> UserNotifications(
            NotificationPlatform? platform,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] bool markAsRead = false)
        {
            if (pageNumber < 1)
                return ApiResponseFactory.BadRequest("invalid pageSize, page size should start from 1.");

            var customerId = await _workContext.CustomerIdAsync();

            var userNotifies = await _notificationService
                .GetNotificationAsync(customerId, platform, pageSize: pageSize, pageIndex: pageNumber);

            if (markAsRead)
            {
                foreach (var notify in userNotifies.Where(notify => !notify.IsRead))
                {
                    await _notificationService.MarkAsRead(customerId, notify.CustomerNotificationId);
                    notify.IsRead = true;
                }
            }

            return ApiResponseFactory.Success(userNotifies);
        }

        [HttpGet("/FrontendApi/PushNotification/Customer/Read")]
        public async Task<IActionResult> HasUnReadNotificationAsync()
        {
            var customerId = await _workContext.CustomerIdAsync();
            var hadUnRead = await _notificationService.HasUnReadNotificationAsync(customerId);
            return ApiResponseFactory.Success(hadUnRead);
        }

        [HttpPost("/api-frontend/PushNotification/MarkAsRead/{id}")]
        [HttpPost("/FrontendApi/PushNotification/MarkAsRead/{id}")]
        public async Task<IActionResult> SendNotificationApi([FromRoute] int id)
        {
            var customerId = await _workContext.CustomerIdAsync();
            await _notificationService.MarkAsRead(customerId, id);
            return ApiResponseFactory.Success();
        }

        [HttpPost("/api-frontend/PushNotification/send")]
        [HttpPost("/FrontendApi/PushNotification/send")]
        public async Task<IActionResult> SendNotificationApi([FromBody] SendNotificationModel model)
        {
            if (model == null)
                return ApiResponseFactory.BadRequest("data is not valid");

            var img = model.PictureId > 0 ? await _pictureService.GetPictureUrlAsync(model.PictureId.Value) : "";

            List<string> tokens = null;
            if (model?.CustomerIds?.Count > 0)
                tokens = await _pushNotificationTokenService.GetTokensAsync(customerIds: model.CustomerIds);
            else
            {
                var platform = model.Platform == NotificationPlatform.All ? null : model.Platform;
                tokens = await _pushNotificationTokenService.GetTokensAsync(platform: platform);
            }

            var notification = new Notification
            {
                RegistrationIds = tokens,
                Title = model.Title,
                Body = model.Body,
                Image = img,
                NotificationType = model.NotificationType,
                NotificationPlatform = model.Platform,
                Data = new PushNotificationData
                {
                    Code = model.Code,
                    Link = model.Link,
                    ExtraData = model.ExtraData,
                    NotificationType = model.NotificationType,
                    NotificationScopeType = NotificationScopeType.Normal
                }
            };
            try
            {
                if (model.Schedule)
                {
                    var jobId = _baramjkBackgroundJob.Schedule(() => _pushNotificationService.SendAsync(notification),
                        model.OnDateTime);
                    await _repository.InsertAsync(new ScheduleNotificationModel
                    {
                        JobId = jobId,
                        OnDateTime = model.OnDateTime,
                        Title = model.Title
                    });

                }
                else
                {
                    await _pushNotificationService.SendAsync(notification);
                }

                return ApiResponseFactory.Success();
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return ApiResponseFactory.InternalServerError("Send notification failed.");
            }
            

           
        }
    }
}
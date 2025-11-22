using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEventTimeSlots;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Media;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class ActorEventTimeSlotsAdminFactory
    {
        private readonly ActorEventTimeSlotService _actorEventTimeSlotService;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;


        public ActorEventTimeSlotsAdminFactory(ActorEventTimeSlotService actorEventTimeSlotService,
            IPictureService pictureService,
            MediaSettings mediaSettings)
        {
            _actorEventTimeSlotService = actorEventTimeSlotService;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        public ActorEventTimeSlotsSearchModel PrepareSearchModel(int timeSlotId, DateTime timeSlotDate)
        {
            var model = new ActorEventTimeSlotsSearchModel()
            {
                TimeSlotId = timeSlotId,
                TimeSlotDateTime = timeSlotDate,
            };
            model.SetGridPageSize();
            return model;
        }


        public async Task<ActorEventTimeSlotsListModel> PrepareListModelAsync(ActorEventTimeSlotsSearchModel searchModel)
        {
            var entities = await _actorEventTimeSlotService.GetAllAsync(searchModel.TimeSlotId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            return await new ActorEventTimeSlotsListModel().PrepareToGridAsync(searchModel, entities, () =>
            {
                return entities.SelectAwait(async x => new ActorEventTimeSlotsItemModel()
                {
                    Id = x.Id,
                    ActorName = x.ActorName,
                    ActorPictureUrl = await _pictureService.GetPictureUrlAsync(x.ActorPicture?.PictureId ?? 0, _mediaSettings.CartThumbPictureSize),
                    Active = x.Active,
                    TimeSlotId = x.TimeSlotId,
                    ActorEventId = x.ActorEventId,
                });
            });
        }
    }
}
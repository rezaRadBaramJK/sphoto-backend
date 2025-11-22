using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.ActorEvents;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Events;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class ActorEventAdminFactory

    {
        private readonly ActorEventService _actorEventService;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ActorAdminFactory _actorAdminFactory;
        private readonly ActorService _actorService;

        public ActorEventAdminFactory(ActorEventService actorEventService,
            IPictureService pictureService,
            MediaSettings mediaSettings,
            ICustomerService customerService,
            ILocalizationService localizationService,
            ActorAdminFactory actorAdminFactory,
            ActorService actorService)
        {
            _actorEventService = actorEventService;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _customerService = customerService;
            _localizationService = localizationService;
            _actorAdminFactory = actorAdminFactory;
            _actorService = actorService;
        }

        public EventActorEventsViewModel PrepareEventActorEventsViewModel(int eventId)
        {
            var viewModel = new EventActorEventsViewModel()
            {
                EventId = eventId,
            };
            viewModel.SetGridPageSize();

            return viewModel;
        }

        public async Task<ActorEventsListModel> PrepareEventActorEventsListModelAsync(EventActorEventsViewModel viewModel)
        {
            var pageIndex = viewModel.Page - 1;
            var actorEvents = await _actorEventService.GetActorEventsDetailsAsync(viewModel.EventId, pageIndex, viewModel.PageSize);

            return await new ActorEventsListModel().PrepareToGridAsync(viewModel, actorEvents, () =>
            {
                return actorEvents.SelectAwait(async item => new ActorEventsItemModel()
                    {
                        Id = item.ActorEvent.Id,
                        EventId = item.ActorEvent.EventId,
                        ActorId = item.ActorEvent.ActorId,
                        ActorName = item.Actor.Name,
                        CommissionAmount = item.ActorEvent.CommissionAmount,
                        ActorPictureUrl =
                            await _pictureService.GetPictureUrlAsync(item.ActorPicture?.PictureId ?? 0, _mediaSettings.CartThumbPictureSize),
                        CameraManEachPictureCost = item.ActorEvent.CameraManEachPictureCost,
                        CustomerMobileEachPictureCost = item.ActorEvent.CustomerMobileEachPictureCost,
                        DisplayOrder = item.ActorEvent.DisplayOrder,
                        ActorShare = item.ActorEvent.ActorShare,
                        ProductionShare = item.ActorEvent.ProductionShare,
                        // this is a temporary mapping
                        ActorPhotoPrice = item.ActorEvent.CameraManEachPictureCost
                    })
                    .OrderBy(ae => ae.DisplayOrder);
            });
        }


        public ActorSearchModel PrepareAddActorViewModel(int eventId)
        {
            var model = new ActorSearchModel();
            model.SetGridPageSize();
            return model;
        }

        public BulkSharesEditViewModel PrepareBulkSharesEditViewModel(int eventId)
        {
            return new BulkSharesEditViewModel() { EventId = eventId };
        }


        public async Task<ActorListModel> PrepareEventActorListModelAsync(int eventId, ActorSearchModel searchModel)
        {
            var pageIndex = searchModel.Page - 1;

            var actors = await _actorService.GetNotAssociatedActorsAsync(eventId, searchModel.SearchEmail, searchModel.SearchName, pageIndex,
                searchModel.PageSize);


            var customers = await _customerService.GetCustomersByIdsAsync(actors.Select(x => x.CustomerId).ToArray());

            return await new ActorListModel().PrepareToGridAsync(searchModel, actors, () =>
            {
                return actors.SelectAwait(async actor => new ActorListItemModel
                {
                    Id = actor.Id,
                    Name = await _localizationService.GetLocalizedAsync(actor, a => a.Name),
                    PictureUrl = await _actorAdminFactory.PreparePictureUrlAsync(actor),
                    DefaultCameraManEachPictureCost = actor.DefaultCameraManEachPictureCost,
                    DefaultCustomerMobileEachPictureCost = actor.DefaultCustomerMobileEachPictureCost,
                    Email = customers.First(c => c.Id == actor.CustomerId).Email
                });
            });
        }
    }
}
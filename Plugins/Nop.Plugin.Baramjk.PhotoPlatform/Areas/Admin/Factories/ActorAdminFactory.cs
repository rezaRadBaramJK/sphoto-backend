using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Models.Actors;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Extensions;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;
using Nop.Plugin.Baramjk.PhotoPlatform.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Factories
{
    public class ActorAdminFactory
    {
        private readonly ActorService _actorService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly MediaSettings _mediaSettings;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;

        public ActorAdminFactory(
            ActorService actorService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            MediaSettings mediaSettings,
            IStaticCacheManager staticCacheManager,
            IPictureService pictureService,
            IWebHelper webHelper,
            ICustomerService customerService)
        {
            _actorService = actorService;
            _localizationService = localizationService;
            _languageService = languageService;
            _mediaSettings = mediaSettings;
            _staticCacheManager = staticCacheManager;
            _pictureService = pictureService;
            _webHelper = webHelper;
            _customerService = customerService;
        }

        public ActorSearchModel PrepareSearchModel()
        {
            return new ActorSearchModel();
        }


        public async Task<ActorListModel> PrepareListModelAsync(ActorSearchModel searchModel)
        {
            var pageIndex = searchModel.Page - 1;
            var actors = await _actorService.GetAllAsync(searchModel.SearchEmail, searchModel.SearchName, pageIndex: pageIndex,
                pageSize: searchModel.PageSize);

            var customers = await _customerService.GetCustomersByIdsAsync(actors.Select(x => x.CustomerId).ToArray());

            return await new ActorListModel().PrepareToGridAsync(searchModel, actors, () =>
            {
                return actors.SelectAwait(async actor => new ActorListItemModel
                {
                    Id = actor.Id,
                    Name = await _localizationService.GetLocalizedAsync(actor, a => a.Name),
                    PictureUrl = await PreparePictureUrlAsync(actor),
                    DefaultCameraManEachPictureCost = actor.DefaultCameraManEachPictureCost,
                    DefaultCustomerMobileEachPictureCost = actor.DefaultCustomerMobileEachPictureCost,
                    Email = customers.FirstOrDefault(c => c.Id == actor.CustomerId)?.Email,
                    CardNumber = actor.CardNumber,
                });
            });
        }


        public async Task<string> PreparePictureUrlAsync(Actor actor)
        {
            if (actor == null)
                throw new ArgumentNullException(nameof(actor));

            var defaultPictureSize = _mediaSettings.CartThumbPictureSize;

            var actorPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                DefaultValues.ActorPictureModelKey,
                actor,
                defaultPictureSize,
                false,
                _webHelper.IsCurrentConnectionSecured());

            return await _staticCacheManager.GetAsync(actorPicturesCacheKey, async () =>
            {
                var pic = (await _actorService.GetPicturesByActorIdAsync(actor.Id, 1)).FirstOrDefault();
                var (url, _) = await _pictureService.GetPictureUrlAsync(pic, defaultPictureSize);
                return url;
            });
        }


        public async Task<ActorViewModel> PrepareViewModelAsync(Actor actor = null)
        {
            var availableLanguages = await _languageService.GetAllLanguagesAsync(true);

            if (actor == null)
                return new ActorViewModel
                {
                    Locales = availableLanguages.Select(lang => new ActorLocalizedModel
                    {
                        LanguageId = lang.Id,
                    }).ToList()
                };

            var viewModel = actor.Map<ActorViewModel>();
            viewModel.Locales = await availableLanguages.SelectAwait(async lang => new ActorLocalizedModel
            {
                LanguageId = lang.Id,
                Name = await _localizationService.GetLocalizedAsync(actor, a => a.Name, lang.Id, false, false),
            }).ToListAsync();


            var customer = await _customerService.GetCustomerByIdAsync(actor.CustomerId);


            viewModel.Email = customer.Email;
            viewModel.CustomerId = customer.Id;
            viewModel.ActorPictureSearchModel.SetGridPageSize();

            return viewModel;
        }

        public async Task<CustomerDetailsWidgetModel> PrepareCustomerDetailsWidgetModelAsync(int customerId)
        {
            var actor = await _actorService.GetByCustomerIdAsync(customerId);
            if (actor == null)
            {
                return new CustomerDetailsWidgetModel();
            }

            return new CustomerDetailsWidgetModel
            {
                ActorId = actor.Id,
                CustomerId = actor.CustomerId,
                ActorLink = $"/Admin/PhotoPlatform/Actor/Edit/{actor.Id}",
            };
        }

        public async Task<ActorPictureListModel> PrepareActorPictureListModelAsync(ActorPictureSearchModel searchModel)
        {
            var pageIndex = searchModel.Page - 1;
            var actorPictures = await _actorService.GetActorPicturesAsync(searchModel.ActorId, pageIndex, searchModel.PageSize);


            return await new ActorPictureListModel().PrepareToGridAsync(searchModel, actorPictures, () =>
            {
                return actorPictures.SelectAwait(async actorPicture => new ActorPictureItemModel()
                {
                    Id = actorPicture.Id,
                    PictureUrl = await _pictureService.GetPictureUrlAsync(actorPicture.PictureId, _mediaSettings.AvatarPictureSize),
                    ActorId = actorPicture.ActorId,
                    DisplayOrder = actorPicture.DisplayOrder,
                });
            });
        }
    }
}
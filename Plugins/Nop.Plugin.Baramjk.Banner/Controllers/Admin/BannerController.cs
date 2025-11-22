using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Banner.Factories;
using Nop.Plugin.Baramjk.Banner.Models;
using Nop.Plugin.Baramjk.Banner.Plugins;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework;
using Nop.Web.Framework.Factories;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Admin
{
    [Permission(PermissionProvider.ManagementName)]
    [Area(AreaNames.Admin)]
    public class BannerController : DefaultDomainController<BannerRecord, BannerModel>
    {
        private readonly BannerService _bannerService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly BannerLocalizationService _bannerLocalizationService;
        private readonly IDownloadService _downloadService;
        private readonly BannerFactory _bannerFactory;

        public BannerController(
            BannerService bannerService,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizedEntityService localizedEntityService,
            BannerLocalizationService bannerLocalizationService,
            IDownloadService downloadService,
            BannerFactory bannerFactory) : base(service: bannerService)
        {
            _bannerService = bannerService;
            _localizedModelFactory = localizedModelFactory;
            _localizedEntityService = localizedEntityService;
            _bannerLocalizationService = bannerLocalizationService;
            _downloadService = downloadService;
            _bannerFactory = bannerFactory;
        }

        [HttpGet]
        public override async Task<IActionResult> ListAsync()
        {
            var model = new BannerSearchModel();
            return View("List.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EntityListAsync(BannerSearchModel searchModel)
        {
            var model = await _bannerFactory.PrepareBannerListModelAsync(searchModel);
            return Json(model);
        }

        protected override async Task<IActionResult> BaseAddAsync()
        {
            var bannerModel = new BannerModel
            {
                Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync<BannerLocalizedModel>(),
                AvailableTypes = await _bannerFactory.PrepareBannerTypeSelectListItemsAsync()
            };

            return View($"AddOrEdit.cshtml", bannerModel);
        }

        public override async Task<IActionResult> AddOrEditAsync(BannerModel model)
        {
            BannerRecord banner;
            if (model.Id > 0)
            {
                banner = await _domainService.EditAsync(model);
                _notificationService.SuccessNotification("Success edit item");
            }
            else
            {
                banner = await _domainService.AddAsync(model);
                _notificationService.SuccessNotification("Success add item");
            }

            await UpdateLocalesAsync(banner, model);
            return RedirectToAction("List");
        }

        protected virtual async Task UpdateLocalesAsync(BannerRecord banner, BannerModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(banner,
                    x => x.Title,
                    localized.Title,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(banner,
                    x => x.Text,
                    localized.Text,
                    localized.LanguageId);
            }
        }

        protected override async Task<IActionResult> BaseEditAsync(int id)
        {
            var entity = await _domainService.GetByIdAsync(id);
            var model = await _domainFactory.GetModelAsync(entity);

            var download = await _downloadService.GetDownloadByGuidAsync(Guid.Parse(entity.FileName));
            model.FileId = download.Id;
            model.Url = $"/Admin/Download/DownloadFile?downloadGuid={download.DownloadGuid}";

            model.Locales = await _bannerLocalizationService.PrepareLocalizedModelsAsync(entity);
            model.AvailableTypes = await _bannerFactory.PrepareBannerTypeSelectListItemsAsync();
            return View($"AddOrEdit.cshtml", model);
        }


        protected override Task<List<T>> GetModelsAsync<T>(IEnumerable<BannerRecord> entities)
        {
            return entities.Select(e => MapUtils.Map<T>(e, new JsonSerializerSettings
            {
                DateFormatString = "g",
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            })).ToListAsync();
        }


    }
}

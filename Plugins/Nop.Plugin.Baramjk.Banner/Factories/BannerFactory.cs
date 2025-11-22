using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Banner.Models;
using Nop.Plugin.Baramjk.Banner.Services;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.Banner.Factories
{
    public class BannerFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly BannerService _bannerService;

        public BannerFactory(ILocalizationService localizationService, BannerService bannerService)
        {
            _localizationService = localizationService;
            _bannerService = bannerService;
        }

        public async Task<IList<SelectListItem>> PrepareBannerTypeSelectListItemsAsync()
        {
            return await Enum
                .GetValues<BannerType>()
                .SelectAwait(async bannerType => new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync(bannerType),
                    Value = $"{(int)bannerType}"
                })
                .ToListAsync();
        }

        public virtual async Task<BannerListModel> PrepareBannerListModelAsync(BannerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var entityId = searchModel.SearchEntityId;
            if (searchModel.EntityId > 0)
            {
                entityId = searchModel.EntityId;
            }

            var entityName = searchModel.SearchEntityName;
            if (!string.IsNullOrWhiteSpace(searchModel.EntityName))
            {
                entityName = searchModel.EntityName;
            }

            var banners = await _bannerService.GetAllAsync(
                searchEntityId: entityId,
                searchEntityName: entityName,
                searchTitle: searchModel.SearchTitle,
                searchTag: searchModel.SearchTag,
                searchExpirationDateFrom: searchModel.SearchExpirationDateFrom,
                searchExpirationDateTo: searchModel.SearchExpirationDateTo,
                page: searchModel.Page - 1,
                pageSize: searchModel.PageSize
            );

            Func<IAsyncEnumerable<BannerListItemModel>> dataFillFunction = () => banners.Select(banner =>
                new BannerListItemModel
                {
                    Id = banner.Id,
                    Title = banner.Title,
                    Text = banner.Text,
                    FileName = banner.FileName,
                    Link = banner.Link,
                    Tag = banner.Tag,
                    DisplayOrder = banner.DisplayOrder,
                    EntityName = banner.EntityName,
                    EntityId = banner.EntityId,
                    BannerType = banner.BannerType
                }).ToAsyncEnumerable();

            var model = await new BannerListModel().PrepareToGridAsync(
                searchModel,
                banners,
                dataFillFunction);

            return model;
        }
    }
}
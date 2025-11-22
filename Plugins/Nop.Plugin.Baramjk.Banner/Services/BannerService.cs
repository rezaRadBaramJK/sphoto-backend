using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Banner.Models;
using Nop.Plugin.Baramjk.Banner.Plugins;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Services.DomainServices;
using Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.Banner.Services
{
    public class BannerService : BaseDomainService<BannerRecord, BannerModel>, IEntityAttachmentService
    {
        private readonly BannerSettings _bannerSettings;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<BannerRecord> _bannerRepository;
        private readonly BannerLocalizationService _bannerLocalizationService;

        public BannerService(IRepository<BannerRecord> domainRepository,
            BannerSettings bannerSettings,
            IDownloadService downloadService,
            IRepository<BannerRecord> bannerRepository,
            BannerLocalizationService bannerLocalizationService) : base(domainRepository)
        {
            _bannerSettings = bannerSettings;
            _downloadService = downloadService;
            _bannerRepository = bannerRepository;
            _bannerLocalizationService = bannerLocalizationService;
        }

        public async Task<List<IEntityAttachmentModel>> GetAttachmentsAsync(string entityName = null,
            int? entityId = null, AttachmentType? attachmentType = null, string tag = "", bool includeExpired = false)
        {
            var type = attachmentType == null ? (BannerType?)null : (BannerType)attachmentType;
            var banners = await GetBannersAsync(entityName, entityId, type, tag, includeExpired: includeExpired);
            await _bannerLocalizationService.UpdateLocalizationAsync(banners);
            var bannerModels = banners.Select(record => new BannerModelDto
            {
                Id = record.Id,
                FileName = record.FileName,
                Title = record.Title,
                Text = record.Text,
                Link = record.Link,
                AltText = record.AltText,
                Tag = record.Tag,
                DisplayOrder = record.DisplayOrder,
                BannerType = record.BannerType,
                EntityId = record.EntityId,
                EntityName = record.EntityName,
                FileUrl = $"banner/GetFile/{record.FileName}",
                ExpirationDateTime = record.ExpirationDateTime

            }).ToList();
            var entityModels = bannerModels.Cast<IEntityAttachmentModel>().ToList();
            return entityModels;
        } 
        public async Task<IPagedList<BannerRecord>> GetAllAsync(
             string entityName = null,
             int? entityId = null,
             BannerType? bannerType = null,
             string tag = "",
             List<int?> entityIds = null,
             bool includeExpired = false,
             string searchTitle = null,
             string searchTag = null,
             string searchEntityName = null,
             int? searchEntityId = null,
             DateTime? searchExpirationDateFrom = null,
             DateTime? searchExpirationDateTo = null,
             int page = 0,
             int pageSize = int.MaxValue)
         {
             var query = _bannerRepository.Table;

             if (!string.IsNullOrWhiteSpace(searchTitle))
                 query = query.Where(b => b.Title.Contains(searchTitle));

             if (!string.IsNullOrWhiteSpace(searchTag))
                 query = query.Where(b => b.Tag.Contains(searchTag));

             if (!string.IsNullOrWhiteSpace(searchEntityName))
                 query = query.Where(b => b.EntityName == searchEntityName);

             if (searchEntityId.HasValue)
                 query = query.Where(b => b.EntityId == searchEntityId);

             if (searchExpirationDateFrom.HasValue)
                 query = query.Where(b => b.ExpirationDateTime >= searchExpirationDateFrom.Value);

             if (searchExpirationDateTo.HasValue)
                 query = query.Where(b => b.ExpirationDateTime <= searchExpirationDateTo.Value);

             if (bannerType.HasValue)
                 query = query.Where(b => b.BannerType == bannerType.Value);

             if (entityId.HasValue)
                 query = query.Where(b => b.EntityId == entityId.Value);

             if (!string.IsNullOrEmpty(entityName))
                 query = query.Where(b => b.EntityName == entityName);

             if (!string.IsNullOrEmpty(tag))
                 query = query.Where(b => b.Tag == tag);

             if (entityIds != null)
                 query = query.Where(b => entityIds.Contains(b.EntityId));

             if (!includeExpired)
                 query = query.Where(b => !b.ExpirationDateTime.HasValue || b.ExpirationDateTime > DateTime.UtcNow);

             return await query.OrderByDescending(b => b.DisplayOrder)
                              .ThenByDescending(b => b.Id)
                              .ToPagedListAsync(page, pageSize);
         }


         [Obsolete]
        public async Task<List<BannerRecord>> GetBannersAsync(string entityName = null,
            int? entityId = null, BannerType? bannerType = null, string tag = "", List<int?> entityIds = null,
            bool includeExpired = false, BannerSearchModel searchModel = null)
        {
            var query = _bannerRepository.Table;

            if (searchModel != null)
            {
                if (!string.IsNullOrWhiteSpace(searchModel.SearchTitle))
                    query = query.Where(b => b.Title.Contains(searchModel.SearchTitle));

                if (!string.IsNullOrWhiteSpace(searchModel.SearchTag))
                    query = query.Where(b => b.Tag.Contains(searchModel.SearchTag));

                if (!string.IsNullOrWhiteSpace(searchModel.SearchEntityName))
                    query = query.Where(b => b.EntityName == searchModel.SearchEntityName);

                if (searchModel.SearchEntityId.HasValue)
                    query = query.Where(b => b.EntityId == searchModel.SearchEntityId);

                if (searchModel.SearchExpirationDateFrom.HasValue)
                    query = query.Where(b => b.ExpirationDateTime >= searchModel.SearchExpirationDateFrom.Value);

                if (searchModel.SearchExpirationDateTo.HasValue)
                    query = query.Where(b => b.ExpirationDateTime <= searchModel.SearchExpirationDateTo.Value);
            }

            if (bannerType.HasValue)
                query = query.Where(b => b.BannerType == bannerType.Value);

            if (entityId.HasValue)
                query = query.Where(b => b.EntityId == entityId.Value);

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(b => b.EntityName == entityName);

            if (!string.IsNullOrEmpty(tag))
                query = query.Where(b => b.Tag == tag);

            if (entityIds != null)
                query = query.Where(b => entityIds.Contains(b.EntityId));

            if (!includeExpired)
                query = query.Where(b => !b.ExpirationDateTime.HasValue || b.ExpirationDateTime > DateTime.UtcNow);

            return await query.OrderByDescending(b => b.DisplayOrder)
                             .ThenByDescending(b => b.Id)
                             .ToListAsync();
        }

        public override async Task<BannerRecord> AddAsync(BannerModel model)
        {
            if (model.FileId == 0)
                throw new BadRequestBusinessException("send file");

            var download = await _downloadService.GetDownloadByIdAsync(model.FileId);
            if (download == null)
                throw new BadRequestBusinessException("send file");


            var bannerRecord = ModelToDomain(model);
            bannerRecord.FileName = download.DownloadGuid.ToString();

            await base.AddAsync(bannerRecord);
            return bannerRecord;
        }

        public async Task AddAsync(IList<BannerRecord> bannersToAdd)
        {
            await _bannerRepository.InsertAsync(bannersToAdd);
        }

        public override async Task<BannerRecord> EditAsync(BannerModel model)
        {
            if (model.FileId == 0)
                throw new BadRequestBusinessException("send file");

            var oldBanner = await GetByIdAsync(model.Id);
            if (oldBanner == null)
                throw new NotFoundBusinessException("Item not found");

            var download = await _downloadService.GetDownloadByIdAsync(model.FileId);

            var bannerRecord = ModelToDomain(model);
            bannerRecord.FileName = download != null ? download.DownloadGuid.ToString() : oldBanner.FileName;

            await EditAsync(bannerRecord);

            return bannerRecord;
        }

        public async Task<BannerTagModel> GetTagsAsync()
        {
            var tags = await _bannerRepository.Table.Select(item => item.Tag).Distinct().ToListAsync();
            var bannerTags = new BannerTagModel
            {
                Tags = _bannerSettings.Tags?.Split(","),
                ProductTags = _bannerSettings.ProductTags?.Split(","),
                CategoryTags = _bannerSettings.CategoryTags?.Split(","),
                VendorTags = _bannerSettings.VendorTags?.Split(","),
                BannerUsedTags = tags
            };

            bannerTags.AllTags = new List<string>();
            bannerTags.AllTags.AddRange(bannerTags.Tags ?? Array.Empty<string>());
            bannerTags.AllTags.AddRange(bannerTags.ProductTags ?? Array.Empty<string>());
            bannerTags.AllTags.AddRange(bannerTags.CategoryTags ?? Array.Empty<string>());
            bannerTags.AllTags.AddRange(bannerTags.VendorTags ?? Array.Empty<string>());
            bannerTags.AllTags.AddRange(bannerTags.BannerUsedTags);
            bannerTags.AllTags = bannerTags.AllTags.Distinct().ToList();

            return bannerTags;
        }

        public override async Task<BannerRecord> DeleteAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                return null;
            return await DeleteAsync(banner);
        }

        public async Task<BannerRecord> DeleteAsync(BannerRecord banner)
        {
            if (banner == null)
                return null;

            var download = await _downloadService.GetDownloadByGuidAsync(new Guid(banner.FileName));
            if (download != null)
                await _downloadService.DeleteDownloadAsync(download);

            await _bannerRepository.DeleteAsync(banner);
            return banner;

        }
    }
}
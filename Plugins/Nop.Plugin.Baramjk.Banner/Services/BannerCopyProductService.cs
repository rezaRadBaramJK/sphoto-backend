using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Banner.Domain;

namespace Nop.Plugin.Baramjk.Banner.Services
{
    public class BannerCopyProductService
    {
        
        private readonly BannerService _bannerService;
        private readonly BannerProductAttributeService _bannerProductAttributeService;
        private readonly BannerDownloadService _bannerDownloadService;

        public BannerCopyProductService(
            BannerService bannerService,
            BannerProductAttributeService bannerProductAttributeService,
            BannerDownloadService bannerDownloadService)
        {
            _bannerService = bannerService;
            _bannerProductAttributeService = bannerProductAttributeService;
            _bannerDownloadService = bannerDownloadService;
            
        }

        public async Task CopyProductBannersAsync(int oldProductId, int newProductId)
        {
            if (oldProductId <= 0 || newProductId <= 0)
                return;
            
            var newBanners = new List<BannerRecord>();
            newBanners.AddRange(await GetProductBannersAsync(oldProductId, newProductId));
            newBanners.AddRange(await GetProductAttributeValueBannersAsync(oldProductId, newProductId));
            await DuplicateDownloadAsync(newBanners);
            await _bannerService.AddAsync(newBanners);
            
        }
        
        private async Task<IList<BannerRecord>> GetProductBannersAsync(int oldProductId, int newProductId)
        {
            var bannersToAdd = new List<BannerRecord>();
            var banners = await _bannerService.GetBannersAsync(nameof(Product), oldProductId);
            foreach (var b in banners)
            {
                b.Id = 0;
                b.EntityId = newProductId;
                bannersToAdd.Add(b);
            }

            return bannersToAdd;
        }
        
        private async Task<IList<BannerRecord>> GetProductAttributeValueBannersAsync(int oldProductId, int newProductId)
        {
            var oldProductAttributeValueBanners = await _bannerProductAttributeService.GetProductAttributeValueBannersAsync(oldProductId);
            var newProductAttributeValues = await _bannerProductAttributeService.GetProductAttributeValuesByProductIdAsync(newProductId);
            var bannersToAdd = new List<BannerRecord>();
            foreach (var newProductAttributeValue in newProductAttributeValues)
            {
                var pair = oldProductAttributeValueBanners.FirstOrDefault(bpva => bpva.AttributeValue.Name == newProductAttributeValue.Name);
                if (pair == null)
                    continue;
                pair.Banner.EntityId = newProductAttributeValue.Id;
                pair.Banner.Id = 0;
                bannersToAdd.Add(pair.Banner);
            }

            return bannersToAdd;
        }
        
        private async Task DuplicateDownloadAsync(IList<BannerRecord> newBanners)
        {
            var oldBannerFileNames = newBanners.Select(b => Guid.Parse(b.FileName)).ToList();
            var downloadsToDuplicate = await _bannerDownloadService.GetDownloadsAsync(oldBannerFileNames);
            
            foreach (var download in downloadsToDuplicate)
            {
                download.Id = 0;
                var oldGuid = download.DownloadGuid.ToString();
                var currentBanner = newBanners.FirstOrDefault(b => b.FileName == oldGuid);
                if(currentBanner == null)
                    continue;
                
                download.DownloadGuid = Guid.NewGuid();
                currentBanner.FileName = download.DownloadGuid.ToString();
            }
            await _bannerDownloadService.AddAsync(downloadsToDuplicate);
            
        }
    }
}
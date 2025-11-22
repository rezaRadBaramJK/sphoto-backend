
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Services.Media;
using Nop.Services.Vendors;

namespace Nop.Plugin.Baramjk.Banner.Services
{
    public class BannerVendorService
    {
        private readonly IPictureService _pictureService;
        private readonly IVendorService _vendorService;
        private readonly BannerService _bannerService;
        private readonly INopFileProvider _fileProvider;
        private readonly IDownloadService _downloadService;
        
        public BannerVendorService(
            IPictureService pictureService,
            IVendorService vendorService, 
            BannerService bannerService, 
            INopFileProvider fileProvider, 
            IDownloadService downloadService)
        {
            _pictureService = pictureService;
            _vendorService = vendorService;
            _bannerService = bannerService;
            _fileProvider = fileProvider;
            _downloadService = downloadService;
        }

        public async Task SaveLogoAsync(IFormFile formFile, Vendor vendor)
        {
            var vendorOldPictureId = vendor.PictureId;
            var insertedLogoPic = await _pictureService.InsertPictureAsync(formFile);
            if (insertedLogoPic != null)
            {
                vendor.PictureId = insertedLogoPic.Id;
                await _vendorService.UpdateVendorAsync(vendor);    
            }

            if (vendorOldPictureId > 0 && vendorOldPictureId != vendor.PictureId)
            {
                var vendorOldPicture = await _pictureService.GetPictureByIdAsync(vendorOldPictureId);
                if (vendorOldPicture != null)
                    await _pictureService.DeletePictureAsync(vendorOldPicture);
            }
        }

        public async Task RemoveLogoAsync(Vendor vendor)
        {
            if(vendor.PictureId == 0)
                return;
            
            var vendorOldPicture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
            if (vendorOldPicture != null) 
                await _pictureService.DeletePictureAsync(vendorOldPicture);
            
            vendor.PictureId = 0;
            await _vendorService.UpdateVendorAsync(vendor);
        }

        public async Task SaveBannersAsync(IList<IFormFile> files, Vendor vendor)
        {
            if(files.Any() == false)
                return;
            
            var bannerRecords = new List<BannerRecord>();
            foreach (var fileToUpload in files)
            {
                var fileBinary = await _downloadService.GetDownloadBitsAsync(fileToUpload);
                
                var fileName = fileToUpload.FileName;
                fileName = _fileProvider.GetFileName(fileName);
                
                var fileExtension = _fileProvider.GetFileExtension(fileName);
                if (string.IsNullOrEmpty(fileExtension) == false)
                    fileExtension = fileExtension.ToLowerInvariant();
                
                var download = new Download
                {
                    DownloadGuid = Guid.NewGuid(),
                    UseDownloadUrl = false,
                    DownloadUrl = string.Empty,
                    DownloadBinary = fileBinary,
                    ContentType = fileToUpload.ContentType,
                    Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                    Extension = fileExtension,
                    IsNew = true
                };
                await _downloadService.InsertDownloadAsync(download);
                
                bannerRecords.Add(new BannerRecord
                {
                    BannerType = BannerType.Picture,
                    EntityName = nameof(Vendor),
                    EntityId = vendor.Id,
                    FileName = download.DownloadGuid.ToString(),
                });
            }
            await _bannerService.AddAsync(bannerRecords);
        }
        
    }
}
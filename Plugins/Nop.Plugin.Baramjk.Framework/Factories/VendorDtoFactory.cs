using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Models.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Models.Vendors;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public class VendorDtoFactory : IVendorDtoFactory
    {
        private readonly IFavoriteVendorService _favoriteVendorService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        private readonly IRepository<Product> _productRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;

        public VendorDtoFactory(IFavoriteVendorService favoriteVendorService,
            IGenericAttributeService genericAttributeService, ILocalizationService localizationService,
            MediaSettings mediaSettings, IPictureService pictureService, IRepository<Product> productRepository,
            IStaticCacheManager staticCacheManager, IStoreContext storeContext,
            IVendorAttributeParser vendorAttributeParser, IVendorAttributeService vendorAttributeService,
            IWebHelper webHelper, IWorkContext workContext, IVendorService vendorService,
            IUrlRecordService urlRecordService)
        {
            _favoriteVendorService = favoriteVendorService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _pictureService = pictureService;
            _productRepository = productRepository;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _webHelper = webHelper;
            _workContext = workContext;
            _vendorService = vendorService;
            _urlRecordService = urlRecordService;
        }

        public async Task<VendorDto> PrepareVendorDtoAsync(int id, bool attributes = true, bool favCount = true,
            bool rating = true, bool genericAttribute = true, bool checkFavourite = true)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            return await PrepareVendorDtoAsync(vendor, attributes, favCount, rating, genericAttribute);
        }

        public async Task<VendorDto> PrepareVendorDtoAsync(Vendor vendor, bool attributes = true, bool favCount = true,
            bool rating = true, bool genericAttribute = true, bool checkFavourite = true)
        {
            var vendorDto = new VendorDto
            {
                Id = vendor.Id,
                Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(vendor, x => x.Description),
                Email = vendor.Email,
                Picture = await PreparePictureModel(vendor),
                FavCount = favCount ? await _favoriteVendorService.GetFansCountAsync(vendor.Id) : 0,
                Rating = rating ? await GetVendorRating(vendor.Id) : 0
            };

            if (attributes)
            {
                var vendorAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(vendor,
                    NopVendorDefaults.VendorAttributes);
                vendorDto.VendorAttributes =
                    attributes ? await PrepareVendorAttributesAsync(vendorAttributesXml) : null;
            }

            if (genericAttribute)
            {
                var genericAttributes = await _genericAttributeService.GetAttributesForEntityAsync(vendor.Id, "Vendor");
                vendorDto.GenericAttributes = genericAttributes
                    .Select(i => new KeyValuePair<string, string>(i.Key, i.Value)).ToList();
            }

            if (checkFavourite)
                vendorDto.IsFavourite =
                    await _favoriteVendorService.IsFavoriteAsync(await _workContext.CustomerIdAsync(), vendor.Id);

            return vendorDto;
        }

        public async Task<List<VendorDto>> PrepareVendorDtoAsync(IEnumerable<Vendor> vendors, bool attributes = true,
            bool favCount = true, bool rating = true, bool genericAttribute = true, bool checkFavourite = true)
        {
            var vendorsIds = await GetCustomerFavoriteVendorsAsync();
            var vendorDtos = await vendors.SelectAwait(async item =>
                {
                    var vendor = await PrepareVendorDtoAsync(item, checkFavourite: false);
                    vendor.IsFavourite = vendorsIds.Contains(vendor.Id);
                    return vendor;
                })
                .ToListAsync();

            return vendorDtos;
        }

        public async Task<VendorBriefInfoModel> VendorBriefInfoModelAsync(int id)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            if (vendor == null)
                return null;

            var vendorModel = new VendorBriefInfoModel
            {
                Id = vendor.Id,
                Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                SeName = await _urlRecordService.GetSeNameAsync(vendor),
            };

            return vendorModel;
        }

        private async Task<HashSet<int>> GetCustomerFavoriteVendorsAsync()
        {
            var customerId = await _workContext.CustomerIdAsync();
            var ids = await _favoriteVendorService.GetCustomerFavoriteVendorsAsync(customerId);
            return ids.ToHashSet();
        }

        private async Task<PictureModel> PreparePictureModel(Vendor vendor)
        {
            var pictureSize = _mediaSettings.VendorThumbPictureSize;
            var pictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                NopModelCacheDefaults.VendorPictureModelKey,
                vendor, pictureSize, true, await _workContext.GetWorkingLanguageAsync(),
                _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var pictureModel = await _staticCacheManager.GetAsync(pictureCacheKey, async () =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
                string fullSizeImageUrl, imageUrl;

                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = fullSizeImageUrl,
                    ImageUrl = imageUrl,
                    Title = string.Format(
                        await _localizationService.GetResourceAsync("Media.Vendor.ImageLinkTitleFormat"), vendor.Name),
                    AlternateText =
                        string.Format(
                            await _localizationService.GetResourceAsync("Media.Vendor.ImageAlternateTextFormat"),
                            vendor.Name)
                };

                return pictureModel;
            });

            return pictureModel;
        }

        protected async Task<IList<VendorAttributeModel>> PrepareVendorAttributesAsync(string vendorAttributesXml)
        {
            var result = new List<VendorAttributeModel>();

            var vendorAttributes = await _vendorAttributeService.GetAllVendorAttributesAsync();
            foreach (var attribute in vendorAttributes)
            {
                var attributeModel = new VendorAttributeModel
                {
                    Id = attribute.Id,
                    Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _vendorAttributeService.GetVendorAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new VendorAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                    {
                        if (!string.IsNullOrEmpty(vendorAttributesXml))
                        {
                            //clear default selection
                            foreach (var item in attributeModel.Values)
                                item.IsPreSelected = false;

                            //select new values
                            var selectedValues =
                                await _vendorAttributeParser.ParseVendorAttributeValuesAsync(vendorAttributesXml);
                            foreach (var attributeValue in selectedValues)
                            foreach (var item in attributeModel.Values)
                                if (attributeValue.Id == item.Id)
                                    item.IsPreSelected = true;
                        }
                    }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //do nothing
                        //values are already pre-set
                    }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    {
                        if (!string.IsNullOrEmpty(vendorAttributesXml))
                        {
                            var enteredText = _vendorAttributeParser.ParseValues(vendorAttributesXml, attribute.Id);
                            if (enteredText.Any())
                                attributeModel.DefaultValue = enteredText[0];
                        }
                    }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //not supported attribute control types
                        break;
                }

                result.Add(attributeModel);
            }

            return result;
        }

        protected async Task<int> GetVendorRating(int vendorId)
        {
            var any = await _productRepository.Table.AnyAsync(item => item.VendorId == vendorId);
            if (any == false)
                return 0;

            var average = await _productRepository.Table
                .Where(item => item.VendorId == vendorId)
                .AverageAsync(item => item.ApprovedRatingSum);

            return (int)average;
        }
    }
}
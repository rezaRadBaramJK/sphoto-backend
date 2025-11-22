using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Baramjk.Framework.Infrastructures.Cache;
using Nop.Services.Media;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public class PictureModelFactory : IPictureModelFactory
    {
        private readonly IPictureService _pictureService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;

        public PictureModelFactory(
            IPictureService pictureService,
            IStaticCacheManager staticCacheManager,
            IWebHelper webHelper,
            IStoreContext storeContext)
        {
            _pictureService = pictureService;
            _staticCacheManager = staticCacheManager;
            _webHelper = webHelper;
            _storeContext = storeContext;
        }
        
        public async Task<PictureModel> PreparePictureModel(int pictureId, int? pictureSize = null)
        {
            if (pictureSize.HasValue == false)
                pictureSize = 0;
            
            var pictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                BaramjkCacheDefaults.PictureModelKey,
                pictureId,
                pictureSize,
                true,
                _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync());

            var cachedPictures = await _staticCacheManager.GetAsync(pictureCacheKey, async () =>
            {
                var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize.Value);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                
                return new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    Title = picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)
                        ? picture.TitleAttribute
                        : string.Empty,
                    AlternateText = picture != null && !string.IsNullOrEmpty(picture.AltAttribute)
                        ? picture.AltAttribute
                        : string.Empty
                };
                
            });
            
            return cachedPictures;
        }
    }
}
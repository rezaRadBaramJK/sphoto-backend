using Nop.Core.Caching;

namespace Nop.Plugin.Baramjk.Framework.Infrastructures.Cache
{
    public static class BaramjkCacheDefaults
    {
        
        /// <summary>
        /// {0} : pic id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </summary>
        public static CacheKey PictureModelKey => new("Nop.pictures-{0}-{1}-{2}-{3}-{4}");
        
    }
}
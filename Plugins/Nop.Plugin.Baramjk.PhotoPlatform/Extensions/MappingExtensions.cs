using Nop.Core.Infrastructure.Mapper;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Extensions
{
    public static class MappingExtensions
    {
        
        public static TDestination Map<TDestination>(this object source)
        {
            return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
        }
        
        
    }
}
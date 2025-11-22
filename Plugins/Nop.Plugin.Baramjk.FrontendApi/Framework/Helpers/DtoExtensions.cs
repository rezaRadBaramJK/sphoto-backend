using Nop.Core;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Framework.Helpers
{
    public static class DtoExtensions
    {
        public static PagedListDto<TEntity, T> ToPagedListDto<TEntity, T>(this IPagedList<TEntity> items)
            where T : BaseDto
        {
            return new PagedListDto<TEntity, T>(items);
        }
    }
}
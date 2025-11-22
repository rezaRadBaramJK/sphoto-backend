using System;
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper
{
    public abstract class BaseMapperConfiguration : Profile, IOrderedMapperProfile
    {
        /// <summary>
        ///     Order of this mapper implementation
        /// </summary>
        public int Order => 0;

        #region Utilites

        protected virtual void CreateDtoMap<TEntityType, TDtoType>(
            Action<IMappingExpression<TDtoType, TEntityType>> ignoreRule = null)
            where TDtoType : BaseDto
        {
            CreateMap<TEntityType, TDtoType>();
            var map = CreateMap<TDtoType, TEntityType>();

            if (ignoreRule == null)
                return;

            ignoreRule(map);
        }
        
        protected virtual void CreateDtoMapping<TEntityType, TDtoType>()
        where TDtoType: Baramjk.Framework.Dto.Abstractions.BaseDto
        {
            CreateMap<TEntityType, TDtoType>();
            var map = CreateMap<TDtoType, TEntityType>();
        }
        
        #endregion
    }
}
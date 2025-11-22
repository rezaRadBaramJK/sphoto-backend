using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Exceptions;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Services.DomainServices
{
    public class BaseDomainService<TDomain, TDomainModel> : IDomainService<TDomain, TDomainModel>
        where TDomainModel : IDomainModel, new()
        where TDomain : BaseEntity, new()
    {
        private readonly IRepository<TDomain> _domainRepository;

        public BaseDomainService(IRepository<TDomain> domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public virtual async Task<TDomain> AddAsync(TDomainModel model)
        {
            var entity = MapUtils.Map<TDomain>(model);
            await _domainRepository.InsertAsync(entity);
            return entity;
        }

        public virtual async Task<TDomain> AddAsync(TDomain entity)
        {
            await _domainRepository.InsertAsync(entity);
            return entity;
        }

        public virtual async Task<TDomain> GetByIdAsync(int id)
        {
            var entity = await _domainRepository.Table.FirstOrDefaultAsync(item => item.Id == id);
            return entity;
        }

        public virtual async Task<TDomain> EditAsync(TDomainModel model)
        {
            var entityOld = await _domainRepository.Table.FirstOrDefaultAsync(item => item.Id == model.Id);
            if (entityOld == null)
                throw new NotFoundBusinessException("Item not found");

            var entity = MapUtils.Map<TDomain>(model);
            await _domainRepository.UpdateAsync(entity);
            return entity;
        }

        public virtual async Task<TDomain> EditAsync(TDomain entity, bool checkExist = false)
        {
            if (checkExist)
            {
                var entityOld = await _domainRepository.Table.FirstOrDefaultAsync(item => item.Id == entity.Id);
                if (entityOld == null)
                    throw new NotFoundBusinessException("Item not found");
            }

            await _domainRepository.UpdateAsync(entity);
            return entity;
        }

        public virtual async Task<TDomain> DeleteAsync(int id)
        {
            var entity = await _domainRepository.Table.FirstOrDefaultAsync(item => item.Id == id);
            if (entity == null)
                return null;

            await _domainRepository.DeleteAsync(entity);
            return entity;
        }

        public virtual async Task<List<TDomain>> GetListAsync()
        {
            var entities = await _domainRepository.Table
                .OrderByDescending(item => item.Id)
                .ToListAsync();

            return entities;
        }

        public virtual async Task<List<TDomain>> GetListAsync(IPagingRequestModel pagingRequestModel)
        {
            var page = Math.Max(pagingRequestModel.Page - 1, 0);
            var size = Math.Max(pagingRequestModel.PageSize, 1);

            var entities = await _domainRepository.Table
                .Skip(page * size).Take(size)
                .OrderByDescending(item => item.Id)
                .ToListAsync();

            return entities;
        }

        public IQueryable<TDomain> GetQueryable(bool descending = true) =>
            descending ? _domainRepository.Table.OrderByDescending(item => item.Id) : _domainRepository.Table;

        protected TDomain ModelToDomain(TDomainModel model) => MapUtils.Map<TDomain>(model);
        protected TTo MapTo<TTo>(object model) => MapUtils.Map<TTo>(model);
    }
}
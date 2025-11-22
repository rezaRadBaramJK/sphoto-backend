using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models.Products;
using Nop.Services.Common;

namespace Nop.Plugin.Baramjk.Framework.Services.Products
{
    public class ProductVisitService : IProductVisitService
    {
        private const string GroupKey = "Product";
        private const string Key = "Visit";
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<Product> _repositoryProduct;

        public ProductVisitService(IGenericAttributeService genericAttributeService,
            IRepository<GenericAttribute> genericAttributeRepository, IRepository<Product> repositoryProduct)
        {
            _genericAttributeService = genericAttributeService;
            _genericAttributeRepository = genericAttributeRepository;
            _repositoryProduct = repositoryProduct;
        }

        public async Task<List<ProductVisitDto>> GetProductsVisitAsync(IEnumerable<int> productIds)
        {
            var query = from ids in productIds
                join attribute in _genericAttributeRepository.Table
                        .Where(item => item.Key == Key && item.KeyGroup == GroupKey)
                    on ids equals attribute.EntityId into intoAttribute
                from firstAattribute in intoAttribute.DefaultIfEmpty()
                select new ProductVisitDto { ProductId = ids, Count = int.Parse(firstAattribute?.Value ?? "0") };

            var attributes =await query.ToListAsync();
            return attributes;
        }

        public async Task<List<ProductVisitDto>> GetProductsMostVisitedAsync(int pageNumber = 0, int pageSize = 25)
        {
            var skip = pageNumber * pageSize;

            var visitModels = await _genericAttributeRepository.Table
                .OrderByDescending(item => int.Parse(item.Value))
                .Where(item => item.Key == Key && item.KeyGroup == GroupKey)
                .Skip(skip)
                .Take(pageSize)
                .Select(item => new ProductVisitDto
                {
                    Count = int.Parse(item.Value),
                    ProductId = item.EntityId
                })
                .ToListAsync();

            return visitModels;
        }

        public async Task<List<ProductVisitDto>> GetProductsMostVisitedAsync(int vendorId, int pageNumber = 0,
            int pageSize = 25)
        {
            var skip = pageNumber * pageSize;
            var queryable = _repositoryProduct.Table.Where(item => item.VendorId == vendorId);
            var visitModels = await _genericAttributeRepository.Table
                .Join(queryable, item => item.EntityId, item => item.Id, (item, ii) => item)
                .OrderByDescending(item => int.Parse(item.Value))
                .Where(item => item.Key == Key && item.KeyGroup == GroupKey)
                .Skip(skip)
                .Take(pageSize)
                .Select(item => new ProductVisitDto
                {
                    Count = int.Parse(item.Value),
                    ProductId = item.EntityId
                })
                .ToListAsync();

            return visitModels;
        }

        public async Task UpdateVisit(int productId, int count)
        {
            var visit = await GetVisitAttrAsync(productId);

            if (visit != null)
            {
                visit.Value = count.ToString();
                await _genericAttributeService.UpdateAttributeAsync(visit);
            }

            await InsertVisitAsync(productId);
        }

        public async Task InsertVisitAsync(int productId)
        {
            var genericAttribute = new GenericAttribute
            {
                Key = Key,
                Value = "1",
                EntityId = productId,
                KeyGroup = GroupKey,
                StoreId = 0
            };

            await _genericAttributeService.InsertAttributeAsync(genericAttribute);
        }

        public async Task<int> IncreaseVisitAsync(int productId)
        {
            var visit = await GetVisitAttrAsync(productId);

            if (visit != null)
            {
                var count = int.Parse(visit.Value) + 1;
                visit.Value = count.ToString();
                await _genericAttributeService.UpdateAttributeAsync(visit);
                return count;
            }

            await InsertVisitAsync(productId);
            return 1;
        }

        public async Task<int> GetVisitAsync(int productId)
        {
            var visit = (await _genericAttributeService.GetAttributesForEntityAsync(productId, GroupKey))
                .FirstOrDefault(item => item.Key == Key);

            if (visit == null)
                return 0;

            int.TryParse(visit.Value, out var visitCount);
            return visitCount;
        }

        private async Task<GenericAttribute> GetVisitAttrAsync(int productId)
        {
            var visit = (await _genericAttributeService.GetAttributesForEntityAsync(productId, GroupKey))
                .FirstOrDefault(item => item.Key == Key);

            return visit;
        }
    }
}
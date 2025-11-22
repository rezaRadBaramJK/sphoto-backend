using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Seo;
using Nop.Data;
using System.Linq;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Baramjk.FrontendApi.Models.Products;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class FrontendProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<CrossSellProduct> _crossSellProductRepository;

        public FrontendProductService(
            IRepository<Product> productRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<CrossSellProduct> crossSellProductRepository)
        {
            _productRepository = productRepository;
            _urlRecordRepository = urlRecordRepository;
            _crossSellProductRepository = crossSellProductRepository;
        }

        public Task<Product> GetProductBySeNameAsync(string seName)
        {
            var query =
                from urlRecord in _urlRecordRepository.Table
                where urlRecord.EntityName == nameof(Product) && urlRecord.Slug == seName && urlRecord.IsActive
                join product in _productRepository.Table on urlRecord.EntityId equals product.Id
                select product;

            return query.FirstOrDefaultAsync();
        }

        public Task<ProductUrlRecordPair> GetProductUrlRecordBySeNameAsync(string seName)
        {
            var query =
                from urlRecord in _urlRecordRepository.Table
                where urlRecord.EntityName == nameof(Product) && urlRecord.Slug == seName && urlRecord.IsActive
                join product in _productRepository.Table on urlRecord.EntityId equals product.Id
                select new ProductUrlRecordPair
                {
                    Product = product,
                    UrlRecord = urlRecord
                };

            return query.FirstOrDefaultAsync();
            
        }

        public async Task<List<string>> GetProductsSeNamesAsync(Language language)
        {
            var query =
                from product in _productRepository.Table
                where product.Deleted == false && product.Published
                join urlRecord in _urlRecordRepository.Table.Where(r => r.EntityName == nameof(Product))
                    on product.Id equals urlRecord.EntityId
                select new
                {
                    ProductId = product.Id,
                    urlRecord.Slug,
                    urlRecord.LanguageId,
                };

            var slugs = await query.ToListAsync();

            return slugs
                .GroupBy(arg => arg.ProductId)
                .Select(grouping =>
                {
                    var item = grouping.FirstOrDefault(g => g.LanguageId == language.Id);
                    if (item != null)
                        return item.Slug;

                    item = grouping.FirstOrDefault(g => g.LanguageId == 0);
                    return item != null 
                        ? item.Slug 
                        : grouping.First().Slug;
                    
                }).ToList();

        }

        public Task<List<Product>> GetCrossSellProductsAsync(int productId)
        {
            var query =
                from crossSellProduct in _crossSellProductRepository.Table 
                where crossSellProduct.ProductId1 == productId
                join product in _productRepository.Table on crossSellProduct.ProductId2 equals product.Id
                where product.Deleted == false && product.Published && product.VisibleIndividually
                select product;

            return query.Distinct().OrderBy(p => p.DisplayOrder).ToListAsync();
            
        }
    }
}
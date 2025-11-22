using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using System.Linq;
using Nop.Plugin.Baramjk.Banner.Domain;
using Nop.Plugin.Baramjk.Banner.Models;

namespace Nop.Plugin.Baramjk.Banner.Services
{
    public class BannerProductAttributeService
    {
        private readonly IRepository<ProductAttributeValue> _productAttributeValuesRepository;
        private readonly IRepository<BannerRecord> _bannerRepository;
        private readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        private readonly IRepository<Product> _productRepository;

        public BannerProductAttributeService(
            IRepository<ProductAttributeValue> productAttributeValuesRepository,
            IRepository<BannerRecord> bannerRepository,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<Product> productRepository)
        {
            _productAttributeValuesRepository = productAttributeValuesRepository;
            _bannerRepository = bannerRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productRepository = productRepository;
        }
        

        public async Task<IList<BannerAttributeValuePair>> GetProductAttributeValueBannersAsync(int productId)
        {
            var banners =  
                from productAttributeValue in _productAttributeValuesRepository.Table
                join productAttributeMapping in _productAttributeMappingRepository.Table on productAttributeValue.ProductAttributeMappingId equals productAttributeMapping.Id
                join product in _productRepository.Table on productAttributeMapping.ProductId equals product.Id 
                where product.Id == productId
                join banner in _bannerRepository.Table on productAttributeValue.Id equals banner.EntityId
                where banner.EntityName == nameof(ProductAttributeValue)
                select new BannerAttributeValuePair
                {
                    Banner = banner,
                    AttributeValue = productAttributeValue
                };

            return await banners.ToListAsync();
        }

        public async Task<IList<ProductAttributeValue>> GetProductAttributeValuesByProductIdAsync(int productId)
        {
            var valuesQuery =
                from productAttributeValue in _productAttributeValuesRepository.Table
                join productAttributeMapping in _productAttributeMappingRepository.Table on productAttributeValue
                    .ProductAttributeMappingId equals productAttributeMapping.Id
                join product in _productRepository.Table on productAttributeMapping.ProductId equals product.Id
                where product.Id == productId
                select productAttributeValue;

            return await valuesQuery.ToListAsync();

        }


    }
}
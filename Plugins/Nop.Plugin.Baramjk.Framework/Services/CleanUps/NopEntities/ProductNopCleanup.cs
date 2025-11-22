using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.CleanUps.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Services.CleanUps.NopEntities
{
    public class ProductNopCleanup : INopCleanup
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;

        public ProductNopCleanup(
            IRepository<Product> productRepository,
            IRepository<ProductPicture> productPictureRepository)
        {
            _productRepository = productRepository;
            _productPictureRepository = productPictureRepository;
        }

        public IQueryable<int> GetPictureIdsQuery()
        {
            var query =
                from product in _productRepository.Table
                where product.Deleted == false
                join productPicture in _productPictureRepository.Table on product.Id equals productPicture.ProductId
                select productPicture.PictureId;

            return query;
        }
    }
}
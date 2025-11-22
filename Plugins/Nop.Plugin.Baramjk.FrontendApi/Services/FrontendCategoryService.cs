using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.Categories;
using System.Linq;
using Nop.Core;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class FrontendCategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;

        public FrontendCategoryService(
            IRepository<Category> categoryRepository,
            IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId, int productCount)
        {
            var query =
                from category in _categoryRepository.Table
                where category.Id == categoryId && category.Deleted == false && category.Published
                join productCategory in _productCategoryRepository.Table on category.Id equals productCategory.CategoryId
                join product in _productRepository.Table on productCategory.ProductId equals product.Id
                where product.Published && product.Deleted == false
                select new
                {
                    Product = product,
                    ProductCategory = productCategory
                };
            
            return await query
                .OrderBy(pc => pc.ProductCategory.DisplayOrder)
                .ThenBy(pc => pc.Product.DisplayOrder)
                .Take(productCount)
                .Select(pc => pc.Product)
                .ToListAsync();
        }

        public async Task<IPagedList<CategoryProductPair>> GetCategoriesProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            int productLimit = int.MaxValue,
            int[] categoriesIds = null)
        {
            var categoriesQuery = _categoryRepository.Table
                .Where(category => (categoriesIds == null || categoriesIds.Contains(category.Id)) && category.Deleted == false &&
                                   category.Published && category.ParentCategoryId == 0)
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

            var query =
                from category in categoriesQuery
                join productCategory in _productCategoryRepository.Table on category.Id equals productCategory.CategoryId
                join product in _productRepository.Table on productCategory.ProductId equals product.Id
                where product.Published && product.Deleted == false
                select new
                {
                    Category = category,
                    Product = product,
                    ProductCategory = productCategory
                };

            var queryResult = await query
                .OrderBy(pc => pc.ProductCategory.DisplayOrder)
                .ThenBy(pc => pc.Product.DisplayOrder)
                .ToListAsync();

            var results = queryResult.GroupBy(pc => pc.Category.Id)
                .Select(pairs => new CategoryProductPair
                {
                    Category = pairs.First().Category,
                    Products = pairs.Select(pc => pc.Product).Take(productLimit).ToList()
                }).ToList();

            var count = await _categoryRepository.Table
                .Where(category => category.Deleted == false && category.Published && category.ParentCategoryId == 0)
                .CountAsync();

            return new PagedList<CategoryProductPair>(results, pageIndex, pageSize, count);
        }
    }
}
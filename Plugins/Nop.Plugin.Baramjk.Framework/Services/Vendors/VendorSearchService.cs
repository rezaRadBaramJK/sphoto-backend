using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Vendors;
using Nop.Data;

namespace Nop.Plugin.Baramjk.Framework.Services.Vendors
{
    public class VendorSearchService : IVendorSearchService
    {
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<GenericAttribute> _repositoryGenericAttribute;

        public VendorSearchService(IRepository<ProductCategory> productCategoryRepository,
            IRepository<Product> productRepository, IRepository<Vendor> vendorRepository,
            IRepository<GenericAttribute> repositoryGenericAttribute)
        {
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _repositoryGenericAttribute = repositoryGenericAttribute;
        }

        public virtual async Task<List<Vendor>> GetAllVendorsAsync(string name = "", string email = "",
            Dictionary<string, string> genericAttribute = null, Dictionary<int, string> attribute = null,
            int? categoryId = 0, List<int> ids = null, int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            var query = _vendorRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(v => v.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(v => v.Email.Contains(email));

            if (!showHidden)
                query = query.Where(v => v.Active);

            if (ids!=null)
                query = query.Where(v => ids.Contains(v.Id));

            query = query.Where(v => !v.Deleted);

            if (genericAttribute != null)
            {
                foreach (var f in genericAttribute)
                {
                    query = query.Where(v => _repositoryGenericAttribute.Table
                        .Any(g => g.KeyGroup == "Vendor" && g.EntityId == v.Id && g.Key == f.Key &&
                                  g.Value == f.Value));
                }
            }

            if (attribute != null)
            {
                foreach (var f in attribute)
                {
                    var id = $"VendorAttribute ID=\"{f.Key}";
                    var value = $"<Value>{f.Value}</Value>";

                    query = query.Where(v => _repositoryGenericAttribute.Table
                        .Any(g => g.KeyGroup == "Vendor" && g.EntityId == v.Id && g.Key == "VendorAttributes" &&
                                  g.Value.Contains(id) && g.Value.Contains(value)));
                }
            }

            if (categoryId is > 0)
            {
                query = from vendor in query
                    join product in _productRepository.Table on vendor.Id equals product.VendorId
                    join productCategory in _productCategoryRepository.Table on product.Id equals productCategory
                        .ProductId
                    where productCategory.CategoryId == categoryId && product.VendorId != 0
                    select vendor;
            }

            query = query.Skip(pageIndex * pageSize).Take(pageSize);
            query = query.OrderBy(v => v.DisplayOrder).ThenBy(v => v.Name).ThenBy(v => v.Email);

            var vendors = await query.ToListAsync();
            return vendors;
        }

        public virtual async Task<List<int>> GetVendorIdsByCategoryIdAsync(int categoryId, int count)
        {
            var query = from productCategory in _productCategoryRepository.Table
                join product in _productRepository.Table on productCategory.ProductId equals product.Id
                where productCategory.CategoryId == categoryId && product.VendorId != 0
                group product by product.VendorId
                into grouping
                orderby grouping.Any()
                select grouping.Key;

            var vendorIds = await query.Take(count).ToListAsync();
            return vendorIds;
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.Vendors;
using Nop.Services.Vendors;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class VendorFactory
    {
        private readonly IVendorSearchService _vendorSearchService;
        private readonly IVendorDtoFactory _vendorDtoFactory;
        private readonly IVendorService _vendorService;

        public VendorFactory(IVendorSearchService vendorSearchService, IVendorDtoFactory vendorDtoFactory,
            IVendorService vendorService)
        {
            _vendorSearchService = vendorSearchService;
            _vendorDtoFactory = vendorDtoFactory;
            _vendorService = vendorService;
        }

        public virtual async Task<List<VendorDto>> GetAllVendorsAsync(SearchVendorRequest request)
        {
            var allVendors = await _vendorSearchService.GetAllVendorsAsync(request.Name,
                request.Email, request.GenericAttribute, request.Attribute, request.CategoryId, request.Ids,
                request.PageIndex, request.PageSize);

            var vendorDtos = await _vendorDtoFactory.PrepareVendorDtoAsync(allVendors);
            return vendorDtos;
        }

        public virtual async Task<List<VendorDto>> PrepareVendorModelsByCategoryIdAsync(int categoryId, int count)
        {
            var allVendors = await _vendorSearchService.GetAllVendorsAsync(categoryId: categoryId, pageSize: count);
            var vendorDtos = await _vendorDtoFactory.PrepareVendorDtoAsync(allVendors);
            return vendorDtos;
        }

        public virtual async Task<List<VendorDto>> GetVendorsAsync(List<int> ids)
        {
            var allVendors = await _vendorSearchService.GetAllVendorsAsync(ids: ids);
            var vendorDtos = await _vendorDtoFactory.PrepareVendorDtoAsync(allVendors);
            return vendorDtos;
        }
    }

    public class SearchVendorRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int? CategoryId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; } = int.MaxValue;
        public bool ShowHidden { get; set; }
        public List<int> Ids { get; set; }
        public Dictionary<string, string> GenericAttribute { get; set; }
        public Dictionary<int, string> Attribute { get; set; }
    }
}
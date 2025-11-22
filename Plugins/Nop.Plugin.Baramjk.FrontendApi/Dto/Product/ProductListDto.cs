using System;
using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models.Products;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Product
{
    public class ProductListDto
    {
        public ProductListDto(IList<ProductOverviewDto> source, int pageIndex, int pageSize, int? totalCount = null)
        {
            pageSize = Math.Max(pageSize, 1);

            TotalCount = totalCount ?? source.Count;
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            PageSize = pageSize;
            PageIndex = pageIndex;

            Products = source;
        }

        public IList<ProductOverviewDto> Products { get; set; }

        public int PageIndex { get; }

        public int PageSize { get; }

        public int TotalCount { get; }

        public int TotalPages { get; }

        public bool HasPreviousPage => PageIndex > 0;

        public bool HasNextPage => PageIndex + 1 < TotalPages;
    }
}
using System.Collections.Generic;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Dto.Abstractions;

namespace Nop.Plugin.Baramjk.Framework.Models.PagedLists
{
    public class CamelCasePagedList<T>: CamelCaseBaseDto
    {
        public CamelCasePagedList()
        {
        }

        public CamelCasePagedList(IPagedList<T> items)
        {
            PageNumber = items.PageIndex + 1;
            PageSize = items.PageSize;
            TotalCount = items.TotalCount;
            TotalPages = items.TotalPages;
            HasPreviousPage = items.HasPreviousPage;
            HasNextPage = items.HasNextPage;

            Items.AddRange(items);
        }
        
        public CamelCasePagedList(List<T> items, int pageNumber, int pageSize, int totalCount)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = TotalCount / pageSize;
            
            if (TotalCount % pageSize > 0)
                TotalPages++;
            
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < TotalPages;

            Items.AddRange(items);
        }
        
        public CamelCasePagedList(IList<T> items, int pageNumber, int pageSize, int totalCount)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = TotalCount / pageSize;
            
            if (TotalCount % pageSize > 0)
                TotalPages++;
            
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < TotalPages;

            Items.AddRange(items);
        }
        
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public bool HasPreviousPage { get; }
        public bool HasNextPage { get; }
        public List<T> Items { get; set; } = new();
        public object Extra { get; set; }
    }
}
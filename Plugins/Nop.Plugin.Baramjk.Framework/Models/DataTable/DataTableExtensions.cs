using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Models.DataTable
{
    public static class DataTableExtensions
    {
        public static DataTableRecordsModel<TModel> PrepareToGrid<TModel>(
            this DataTableRecordsModel<TModel> dataTableModel,
            IEnumerable<TModel> models, int draw = 1, int? totalCount = null)
        {
            dataTableModel.Data = models;
            dataTableModel.Draw = draw;
            dataTableModel.RecordsTotal = totalCount ?? models.Count();
            dataTableModel.RecordsFiltered = totalCount ?? models.Count();
            return dataTableModel;
        }

        public static DataTableRecordsModel<TModel> PrepareToGrid<TModel>(
            this DataTableRecordsModel<TModel> dataTableModel, IQueryable<TModel> query,
            IPagingRequestModel searchModel, int draw = 1, int? totalCount = null)
        {
            if (totalCount == null)
                totalCount = query.Count();

            var rows = searchModel.Pagination(query).ToList();
            dataTableModel.Data = rows;
            dataTableModel.Draw = draw;
            dataTableModel.RecordsTotal = totalCount.Value;
            dataTableModel.RecordsFiltered = totalCount.Value;
            return dataTableModel;
        }

        public static async Task<DataTableRecordsModel<T>> PrepareToGrid<T>(this IPagingRequestModel searchModel,
            IQueryable<T> model, int draw = 1, int? count = null)
        {
            var models =await searchModel.Pagination(model).ToListAsync();
            count ??= model.Count();
            var prepareToGrid = new DataTableRecordsModel<T>().PrepareToGrid(models,draw, count);
            return prepareToGrid;
        }
        
        public static IQueryable<TModel> Pagination<TModel>(this IPagingRequestModel page, IQueryable<TModel> query)
        {
            var pageSize = Math.Max(page.PageSize, 1);
            return query.Skip((page.Page - 1) * pageSize).Take(pageSize);
        }

        [Obsolete]
        public static async Task<TListModel> PrepareToGridAsync<TListModel, TModel, TObject>
        (this TListModel listModel, ExtendedSearchModel searchModel, IPagedList<TObject> objectList,
            Func<IAsyncEnumerable<TModel>> dataFillFunction)
            where TListModel : DataTableRecordsModel<TModel>
        {
            if (listModel == null)
                throw new ArgumentNullException(nameof(listModel));

            listModel.Data = await (dataFillFunction?.Invoke()).ToListAsync();
            listModel.Draw = searchModel?.Draw ?? 0;
            listModel.RecordsTotal = objectList?.TotalCount ?? 0;
            listModel.RecordsFiltered = objectList?.TotalCount ?? 0;

            return listModel;
        }
    }
}
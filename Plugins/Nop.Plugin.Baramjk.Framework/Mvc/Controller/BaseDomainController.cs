using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Services.DomainServices;
using Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Services.Messages;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Baramjk.Framework.Mvc.Controller
{
#pragma warning disable CS1998
    public abstract class BaseDomainController<TDomain, TDomainModel> : BaseBaramjkPluginController
        where TDomainModel : IDomainModel, new()
        where TDomain : BaseEntity, new()
    {
        protected readonly INotificationService _notificationService;
        protected readonly IDataTablesBuilders _dataTablesBuilders;
        protected IDomainService<TDomain, TDomainModel> _domainService;
        protected IDomainFactory<TDomain, TDomainModel> _domainFactory;

        protected BaseDomainController(IDomainService<TDomain, TDomainModel> service = null,
            IDomainFactory<TDomain, TDomainModel> factory = null)
        {
            _notificationService = EngineContext.Current.Resolve<INotificationService>();
            _dataTablesBuilders = EngineContext.Current.Resolve<IDataTablesBuilders>();

            _domainService = service ?? EngineContext.Current.Resolve<IDomainService<TDomain, TDomainModel>>();
            _domainFactory = factory ?? EngineContext.Current.Resolve<IDomainFactory<TDomain, TDomainModel>>();
        }

        protected override string GetViewPath(string viewName)
        {
            return $"~/Plugins/{SystemName}/Views/{FolderName}/{viewName}";
        }

        protected static string GetViewPath(string systemName, string folderName, string viewName)
        {
            return $"~/Plugins/{systemName}/Views/{folderName}/{viewName}";
        }

        protected virtual async Task<IActionResult> BaseAddAsync()
        {
            return View($"AddOrEdit.cshtml", new TDomainModel());
        }

        protected virtual async Task<IActionResult> BaseAddOrEditAsync([FromForm] TDomainModel model)
        {
            if (model.Id > 0)
            {
                await _domainService.EditAsync(model);
                _notificationService.SuccessNotification("Success edit item");
            }
            else
            {
                await _domainService.AddAsync(model);
                _notificationService.SuccessNotification("Success add item");
            }

            return RedirectToAction("List");
        }

        protected virtual async Task<IActionResult> BaseEditAsync(int id)
        {
            var entity = await _domainService.GetByIdAsync(id);
            var model = await _domainFactory.GetModelAsync(entity);
            return View($"AddOrEdit.cshtml", model);
        }

        protected virtual async Task<IActionResult> BaseListAsync()
        {
            return View($"List.cshtml", new ExtendedSearchModel());
        }

        protected virtual async Task<IActionResult> BaseDefaultListAsync(DefaultListViewModel model)
        {
            model.DataTablesModel = await _dataTablesBuilders.BuildDataTablesModelAsync(model);
            return ViewBase(model.ViewFullPath, model);
        }

        protected virtual async Task<IActionResult> BaseListAsync(ExtendedSearchModel searchModel)
        {
            return await PrepareGridResponseAsync(searchModel, _domainService.GetQueryable());
        }

        protected virtual async Task<IActionResult> BaseDeleteAsync([FromForm] DeleteRequest request)
        {
            await _domainService.DeleteAsync(request.Id);
            return new NullJsonResult();
        }

        protected virtual async Task<IActionResult> PrepareGridResponseAsync(ExtendedSearchModel searchModel,
            IQueryable<TDomain> query)
        {
            var entities = searchModel.Pagination(query).ToList();
            var models = await GetModelsAsync(entities);
            var model = new DataTableRecordsModel<TDomainModel>()
                .PrepareToGrid(models, searchModel.NextDraw, query.Count());
            return Json(model);
        }

        protected virtual async Task<IActionResult> PrepareGridResponseAsync<T>(ExtendedSearchModel searchModel,
            IQueryable<TDomain> query)
        {
            var entities = searchModel.Pagination(query).ToList();
            var models = await GetModelsAsync<T>(entities);
            var model = new DataTableRecordsModel<T>().PrepareToGrid(models, searchModel.NextDraw, query.Count());
            return Json(model);
        }

        protected virtual async Task<IActionResult> PrepareGridResponseAsync(ExtendedSearchModel searchModel,
            List<TDomain> entities, int? totalCount = null)
        {
            var models = await GetModelsAsync(entities);
            var model = new DataTableRecordsModel<TDomainModel>()
                .PrepareToGrid(models, searchModel.NextDraw, totalCount);
            return Json(model);
        }

        protected virtual async Task<List<TDomainModel>> GetModelsAsync(IEnumerable<TDomain> entities)
        {
            return await _domainFactory.GetModelsAsync(entities);
        }

        protected virtual async Task<List<T>> GetModelsAsync<T>(IEnumerable<TDomain> entities)
        {
            return await entities.SelectAwait(async (e) => MapUtils.Map<T>(e)).ToListAsync();
        }
    }
}
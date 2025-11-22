using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Models.Search;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    [Permission(PermissionProvider.PushNotificationManagementName)]
    [Area(AreaNames.Admin)]
    public class
        ScheduleListController : DefaultDomainController<ScheduleNotificationModel, ScheduleNotificationDomainModel>
    {
        private IRepository<ScheduleNotificationModel> _repository;

        public async Task<IActionResult> ListAsync(ScheduleSearchModel searchModel)
        {
            var query = _repository.Table.Where(x => x.OnDateTime > DateTime.Now);
            return await PrepareGridResponseAsync(searchModel, query);
        }

        public async Task<IActionResult> GetScheduleList()
        {
            return await BaseDefaultListAsync(await GetListViewModelAsync());
        }

        [HttpGet("/Admin/ScheduleList/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _repository.Table.Where(x => x.Id == id).FirstOrDefaultAsync();
            await _repository.DeleteAsync(record);
            return RedirectToAction("GetScheduleList");
        }

        protected override async Task<List<ScheduleNotificationDomainModel>> GetModelsAsync(
            IEnumerable<ScheduleNotificationModel> entities)
        {
            var models = await _domainFactory.GetModelsAsync(entities);
            return models;
        }


        private static DefaultListViewModel _defaultListViewModel;

        public ScheduleListController(IRepository<ScheduleNotificationModel> repository)
        {
            _repository = repository;
        }


        private async Task<DefaultListViewModel> GetListViewModelAsync()
        {
            return _defaultListViewModel ??=
                new(controllerName: "ScheduleList", systemName: DefaultValue.SystemName,
                    typeof(ScheduleNotificationDomainModel))
                {
                    PageTitle = await GetResAsync("ScheduleList"),
                    DeleteAction = null,
                    UpdateAction = null,
                    AddAction = null,
                    ViewFullPath = GetViewPath("List.cshtml")
                };
        }

        protected override string SystemName => DefaultValue.SystemName;
        protected override string FolderName => "ScheduleList";
    }
}
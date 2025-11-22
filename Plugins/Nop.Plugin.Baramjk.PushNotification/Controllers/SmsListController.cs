using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Services.Sms;
using Nop.Plugin.Baramjk.PushNotification.Models;
using Nop.Plugin.Baramjk.PushNotification.Models.Search;
using Nop.Plugin.Baramjk.PushNotification.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Baramjk.PushNotification.Controllers
{
    [Permission(PermissionProvider.PushNotificationManagementName)]
    [Area(AreaNames.Admin)]
    public class SmsListController : DefaultDomainController<SendSmsModel, SendSmsDomainModel>
    {
        // [HttpPost]
        public async Task<IActionResult> ListAsync(SmsSearchModel searchModel)
        {
            var query = _domainService.GetQueryable()
                .Where(x=>x.Status==searchModel.Status)
                .Where(x=>x.DateTime>searchModel.From && x.DateTime<=searchModel.To);
            return await PrepareGridResponseAsync(searchModel, query);
        }

        // [HttpGet]
        // public override async Task<IActionResult> ListAsync()
        // {
        //     return await BaseDefaultListAsync(await GetListViewModelAsync());
        // } 
        // [HttpGet]
        public async Task<IActionResult> GetSmsList()
        {
            return await BaseDefaultListAsync(await GetListViewModelAsync());
        }

        protected override async Task<List<SendSmsDomainModel>> GetModelsAsync(
            IEnumerable<SendSmsModel> entities)
        {
            var models = await _domainFactory.GetModelsAsync(entities);
            return models;
        }


        private static DefaultListViewModel _defaultListViewModel;
        

        private async Task<DefaultListViewModel> GetListViewModelAsync()
        {
            return _defaultListViewModel ??=
                new(controllerName: "SmsList", systemName: DefaultValue.SystemName, typeof(SendSmsDomainModel))
                {
                    PageTitle = await GetResAsync("SmsList"),
                    DeleteAction = null,
                    UpdateAction = null,
                    AddAction = null,
                    ViewFullPath = GetViewPath("SmsList.cshtml")
                };
        }

        protected override string SystemName => DefaultValue.SystemName;
        protected override string FolderName => "SmsList";
    }
}
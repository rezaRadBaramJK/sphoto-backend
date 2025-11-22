using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Services.DomainServices;
using Nop.Plugin.Baramjk.Framework.Utils;
using Nop.Web.Framework;

#pragma warning disable CS1998

namespace Nop.Plugin.Baramjk.Framework.Mvc.Controller
{
    [Area(AreaNames.Admin)]
    public abstract class DefaultDomainController<TDomain, TDomainModel> : BaseDomainController<TDomain, TDomainModel>
        where TDomainModel : IDomainModel, new()
        where TDomain : BaseEntity, new()
    {
        protected DefaultDomainController(IDomainService<TDomain, TDomainModel> service = null,
            IDomainFactory<TDomain, TDomainModel> factory = null) : base(service, factory)
        {
        }

        public virtual async Task<IActionResult> AddAsync() => await BaseAddAsync();

        [HttpPost]
        public virtual async Task<IActionResult> AddOrEditAsync([FromForm] TDomainModel model) =>
            await BaseAddOrEditAsync(model);

        [HttpGet]
        public virtual async Task<IActionResult> EditAsync(int id) => await BaseEditAsync(id);

        [HttpGet]
        public virtual async Task<IActionResult> ListAsync() => await BaseListAsync();

        [HttpPost]
        public virtual async Task<IActionResult> ListAsync(ExtendedSearchModel searchModel) =>
            await BaseListAsync(searchModel);

        [HttpPost]
        public virtual async Task<IActionResult> DeleteAsync([FromForm] DeleteRequest request) =>
            await BaseDeleteAsync(request);
    }
}
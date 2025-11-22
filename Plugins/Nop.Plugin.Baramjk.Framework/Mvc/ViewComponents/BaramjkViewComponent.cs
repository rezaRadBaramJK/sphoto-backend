using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Mvc.ViewComponents
{
    public abstract class BaramjkViewComponent : NopViewComponent
    {
        protected readonly IPermissionService _permissionService;
        protected readonly IWorkContext _workContext;

        protected BaramjkViewComponent()
        {
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _permissionService = EngineContext.Current.Resolve<IPermissionService>();
        }

        protected virtual string SystemName => GetType().Assembly.GetName().Name?.Replace("Nop.Plugin.", "");

        public virtual async Task<bool> AuthorizeAsync(params PermissionRecord[] permissions)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return await permissions.AnyAwaitAsync(async permission =>
                await _permissionService.AuthorizeAsync(permission, customer));
        }

        public virtual async Task<bool> AuthorizeAsync(PermissionRecord permission)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return await _permissionService.AuthorizeAsync(permission, customer);
        }

        public new Microsoft.AspNetCore.Mvc.ViewComponents.ViewViewComponentResult View(string viewName)
        {
            return base.View($"~/Plugins/{SystemName}/Views/{viewName}");
        }

        public new Microsoft.AspNetCore.Mvc.ViewComponents.ViewViewComponentResult View<TModel>(string viewName,
            TModel model)
        {
            return base.View($"~/Plugins/{SystemName}/Views/{viewName}", model);
        }

        protected virtual Microsoft.AspNetCore.Mvc.ViewComponents.ViewViewComponentResult ViewBase(string viewName)
        {
            return base.View(viewName);
        }

        protected virtual Microsoft.AspNetCore.Mvc.ViewComponents.ViewViewComponentResult ViewBase(string viewName,
            object model)
        {
            return base.View(viewName, model);
        }

        protected int? GetId(object model)
        {
            if (model is FrameworkBaseNopEntityModel entityModel)
                return entityModel.Id;

            return null;
        }
    }
}
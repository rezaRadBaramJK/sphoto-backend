using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Customers;

namespace Nop.Plugin.Baramjk.Banner.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAndVendorAuthorizeAttribute: TypeFilterAttribute
    {
        public bool IgnoreFilter { get; }
    
        public AdminAndVendorAuthorizeAttribute(bool ignore = false) : base(typeof(AuthorizeCustomerFilter))
        {
            IgnoreFilter = ignore;
            Arguments = new object[] { ignore };
        }
        
        
        private class AuthorizeCustomerFilter : IAsyncAuthorizationFilter
        {
            private readonly bool _ignoreFilter;
            private readonly IWorkContext _workContext;
            private readonly ICustomerService _customerService;


            public AuthorizeCustomerFilter(
                bool ignoreFilter, 
                IWorkContext workContext,
                ICustomerService customerService)
            {
                _ignoreFilter = ignoreFilter;
                _workContext = workContext;
                _customerService = customerService;
            }
            
            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                await CheckAccessWebApiAsync(context);
            }
            
            private async Task CheckAccessWebApiAsync(AuthorizationFilterContext context)
            {
                if (context == null)
                    throw new NopException(nameof(context));
                
                //check whether this filter has been overridden for the Action
                var actionFilter = context.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter)
                    .OfType<AdminAndVendorAuthorizeAttribute>()
                    .FirstOrDefault();

                //ignore filter (the action is available even if navigation is not allowed)
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return;

                var customer = await _workContext.GetCurrentCustomerAsync();
                
                //check whether current customer has access to Web API
                if (await _customerService.IsAdminAsync(customer) ||
                    await _customerService.IsVendorAsync(customer))
                    return;

                //customer hasn't access to Web API
                context.Result = new JsonResult(new { Message = "Access denied." })
                    { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }
}
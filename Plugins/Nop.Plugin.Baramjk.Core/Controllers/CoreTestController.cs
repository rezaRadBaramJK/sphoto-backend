using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.Core.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class CoreTestController : BasePluginController
    {
        public async Task<IActionResult> Test1()
        {
            return Ok("ok");
        }
    }
}
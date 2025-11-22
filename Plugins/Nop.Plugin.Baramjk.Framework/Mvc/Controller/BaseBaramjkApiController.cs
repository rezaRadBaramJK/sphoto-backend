using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.Framework.Mvc.Controller
{
    [TypeFilter(typeof(ApiExceptionFilter))]
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseBaramjkApiController : ControllerBase
    {
    }
}
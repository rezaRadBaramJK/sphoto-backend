using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Services.Tasks;
using Task = Nop.Services.Tasks.Task;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class ScheduleTaskController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public ScheduleTaskController(IScheduleTaskService scheduleTaskService)
        {
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Run task
        /// </summary>
        /// <param name="taskType">Task type</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> RunTask([FromQuery] [Required] string taskType)
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(taskType);
            if (scheduleTask == null)
                //schedule task cannot be loaded
                return ApiResponseFactory.NotFound($"Not found the specified task by type={taskType}");

            var task = new Task(scheduleTask);
            await task.ExecuteAsync();

            return ApiResponseFactory.Success();
        }

        #endregion
    }
}
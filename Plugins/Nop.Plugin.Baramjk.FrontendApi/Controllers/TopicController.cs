using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Topics;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class TopicController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly ITopicModelFactory _topicModelFactory;

        #endregion

        #region Ctor

        public TopicController(ITopicModelFactory topicModelFactory)
        {
            _topicModelFactory = topicModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets a topic details
        /// </summary>
        /// <param name="id">The topic identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TopicModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetTopicDetails(int id)
        {
            if (id <= 0)
                return ApiResponseFactory.BadRequest();

            var topicModel = await _topicModelFactory.PrepareTopicModelByIdAsync(id);
            if (topicModel == null)
                return ApiResponseFactory.NotFound($"Topic Id={id} not found");

            var topicDto = topicModel.ToDto<TopicModelDto>();

            return ApiResponseFactory.Success(topicDto);
        }

        #endregion
    }
}
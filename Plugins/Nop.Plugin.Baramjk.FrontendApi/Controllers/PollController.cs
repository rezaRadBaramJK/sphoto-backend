using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Polls;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Poll;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Polls;
using Nop.Services.Stores;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class PollController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public PollController(ICustomerService customerService,
            ILocalizationService localizationService,
            IPollModelFactory pollModelFactory,
            IPollService pollService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _pollModelFactory = pollModelFactory;
            _pollService = pollService;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [HttpGet("{pollAnswerId}")]
        [ProducesResponseType(typeof(PollModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Vote(int pollAnswerId)
        {
            var pollAnswer = await _pollService.GetPollAnswerByIdAsync(pollAnswerId);
            if (pollAnswer == null)
                return ApiResponseFactory.NotFound($"Poll answer Id={pollAnswerId} not found");

            var poll = await _pollService.GetPollByIdAsync(pollAnswer.PollId);

            if (!poll.Published || !await _storeMappingService.AuthorizeAsync(poll))
                return ApiResponseFactory.BadRequest("Poll is not available");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !poll.AllowGuestsToVote)
                return ApiResponseFactory.BadRequest(
                    await _localizationService.GetResourceAsync("Polls.OnlyRegisteredUsersVote"));

            var alreadyVoted =
                await _pollService.AlreadyVotedAsync(poll.Id, (await _workContext.GetCurrentCustomerAsync()).Id);
            if (!alreadyVoted)
            {
                //vote
                await _pollService.InsertPollVotingRecordAsync(new PollVotingRecord
                {
                    PollAnswerId = pollAnswer.Id,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    CreatedOnUtc = DateTime.UtcNow
                });

                //update totals
                pollAnswer.NumberOfVotes =
                    (await _pollService.GetPollVotingRecordsByPollAnswerAsync(pollAnswer.Id)).Count;
                await _pollService.UpdatePollAnswerAsync(pollAnswer);
                await _pollService.UpdatePollAsync(poll);
            }

            var model = await _pollModelFactory.PreparePollModelAsync(poll, true);

            return ApiResponseFactory.Success(model.ToDto<PollModelDto>());
        }

        #endregion

        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IPollModelFactory _pollModelFactory;
        private readonly IPollService _pollService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;

        #endregion
    }
}
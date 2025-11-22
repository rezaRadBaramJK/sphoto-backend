using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Newsletter;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Factories;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class NewsletterController : BaseNopWebApiFrontendController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INewsletterModelFactory _newsletterModelFactory;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;

        public NewsletterController(ILocalizationService localizationService,
            INewsletterModelFactory newsletterModelFactory,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService)
        {
            _localizationService = localizationService;
            _newsletterModelFactory = newsletterModelFactory;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SubscribeNewsletter([FromQuery] [Required] string email,
            [FromQuery] [Required] bool subscribe)
        {
            string result;

            if (!CommonHelper.IsValidEmail(email))
                return ApiResponseFactory.BadRequest(
                    await _localizationService.GetResourceAsync("Newsletter.Email.Wrong"));

            email = email.Trim();

            var subscription =
                await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(email,
                    (await _storeContext.GetCurrentStoreAsync()).Id);
            if (subscription != null)
            {
                if (subscribe)
                {
                    if (!subscription.Active)
                        await _workflowMessageService.SendNewsLetterSubscriptionActivationMessageAsync(subscription,
                            (await _workContext.GetWorkingLanguageAsync()).Id);
                    result = await _localizationService.GetResourceAsync("Newsletter.SubscribeEmailSent");
                }
                else
                {
                    if (subscription.Active)
                        await _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessageAsync(subscription,
                            (await _workContext.GetWorkingLanguageAsync()).Id);
                    result = await _localizationService.GetResourceAsync("Newsletter.UnsubscribeEmailSent");
                }
            }
            else if (subscribe)
            {
                subscription = new NewsLetterSubscription
                {
                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                    Email = email,
                    Active = false,
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(subscription);
                await _workflowMessageService.SendNewsLetterSubscriptionActivationMessageAsync(subscription,
                    (await _workContext.GetWorkingLanguageAsync()).Id);

                result = await _localizationService.GetResourceAsync("Newsletter.SubscribeEmailSent");
            }
            else
            {
                result = await _localizationService.GetResourceAsync("Newsletter.UnsubscribeEmailSent");
            }

            return ApiResponseFactory.Success(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SubscriptionActivationModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> SubscriptionActivation([FromQuery] [Required] Guid token,
            [FromQuery] [Required] bool active)
        {
            var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuidAsync(token);
            if (subscription == null)
                return ApiResponseFactory.NotFound($"Subscription token = {token} not found");

            if (active)
            {
                subscription.Active = true;
                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(subscription);
            }
            else
            {
                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);
            }

            var model = await _newsletterModelFactory.PrepareSubscriptionActivationModelAsync(active);

            return ApiResponseFactory.Success(model.ToDto<SubscriptionActivationModelDto>());
        }
    }
}
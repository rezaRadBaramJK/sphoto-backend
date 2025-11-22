using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;
using Nop.Core.Events;
using Nop.Core.Rss;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto;
using Nop.Plugin.Baramjk.FrontendApi.Dto.News;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.News;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class NewsController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public NewsController(
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            INewsModelFactory newsModelFactory,
            INewsService newsService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            NewsSettings newsSettings)
        {
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _newsModelFactory = newsModelFactory;
            _newsService = newsService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _newsSettings = newsSettings;
        }

        #endregion

        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly INewsModelFactory _newsModelFactory;
        private readonly INewsService _newsService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly NewsSettings _newsSettings;

        #endregion

        #region Methods

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewsItemListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> List([FromBody] BasePageableModelDto command)
        {
            if (!_newsSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_newsSettings.Enabled)} is not enabled");
            
            var filteringModel = new NewsPagingFilteringModel
            {
                PageNumber = command.PageNumber,
                PageSize = command.PageSize,
                TotalItems = command.TotalItems,
                TotalPages = command.TotalPages,
                FirstItem = command.FirstItem,
                LastItem = command.LastItem,
                HasPreviousPage = command.HasPreviousPage,
                HasNextPage = command.HasNextPage
            };
            
            var model =
                await _newsModelFactory.PrepareNewsItemListModelAsync(filteringModel);

            return ApiResponseFactory.Success(model.ToDto<NewsItemListModelDto>());
        }

        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ListRss(int languageId)
        {
            if (!_newsSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_newsSettings.Enabled)} is not enabled");

            var feed = new RssFeed(
                $"{await _localizationService.GetLocalizedAsync(await _storeContext.GetCurrentStoreAsync(), x => x.Name)}: News",
                "News",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            var items = new List<RssItem>();
            var newsItems =
                await _newsService.GetAllNewsAsync(languageId, (await _storeContext.GetCurrentStoreAsync()).Id);
            foreach (var n in newsItems)
            {
                var newsUrl = Url.RouteUrl("NewsItem",
                    new
                    {
                        SeName = await _urlRecordService.GetSeNameAsync(n, n.LanguageId,
                            ensureTwoPublishedLanguages: false)
                    }, _webHelper.GetCurrentRequestProtocol());
                items.Add(new RssItem(n.Title, n.Short, new Uri(newsUrl),
                    $"urn:store:{(await _storeContext.GetCurrentStoreAsync()).Id}:news:blog:{n.Id}", n.CreatedOnUtc));
            }

            feed.Items = items;

            return ApiResponseFactory.Success(feed.GetContent());
        }

        [HttpGet("{newsItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewsItemModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNewsItem(int newsItemId)
        {
            if (!_newsSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_newsSettings.Enabled)} is not enabled");

            var newsItem = await _newsService.GetNewsByIdAsync(newsItemId);
            if (newsItem == null)
                return ApiResponseFactory.NotFound($"News item Id={newsItemId} not found");

            var notAvailable =
                //published?
                !newsItem.Published ||
                //availability dates
                !_newsService.IsNewsAvailable(newsItem) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(newsItem);
            //Check whether the current user has a "Manage news" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                                 await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews);
            if (notAvailable && !hasAdminAccess)
                return ApiResponseFactory.NotFound($"News item Id={newsItemId} not found");

            var model = new NewsItemModel();
            model = await _newsModelFactory.PrepareNewsItemModelAsync(model, newsItem, true);

            return ApiResponseFactory.Success(model.ToDto<NewsItemModelDto>());
        }

        [HttpPost("{newsItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> NewsCommentAdd([FromBody] NewsItemModelDto model, int newsItemId)
        {
            if (!_newsSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_newsSettings.Enabled)} is not enabled");

            var newsItem = await _newsService.GetNewsByIdAsync(newsItemId);

            if (newsItem == null)
                return ApiResponseFactory.NotFound($"News item Id={newsItemId} not found");

            if (!newsItem.Published || !newsItem.AllowComments)
                return ApiResponseFactory.BadRequest(
                    $"The property {nameof(newsItem.AllowComments)} is not allowed or news item is not published.");

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_newsSettings.AllowNotRegisteredUsersToLeaveComments)
                return ApiResponseFactory.BadRequest(
                    await _localizationService.GetResourceAsync("News.Comments.OnlyRegisteredUsersLeaveComments"));

            var modelNews = model.FromDto<NewsItemModel>();

            var comment = new NewsComment
            {
                NewsItemId = newsItem.Id,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                CommentTitle = modelNews.AddNewComment.CommentTitle,
                CommentText = modelNews.AddNewComment.CommentText,
                IsApproved = !_newsSettings.NewsCommentsMustBeApproved,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _newsService.InsertNewsCommentAsync(comment);

            //notify a store owner;
            if (_newsSettings.NotifyAboutNewNewsComments)
                await _workflowMessageService.SendNewsCommentNotificationMessageAsync(comment,
                    _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddNewsComment",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddNewsComment"), comment);

            //raise event
            if (comment.IsApproved)
                await _eventPublisher.PublishAsync(new NewsCommentApprovedEvent(comment));

            return ApiResponseFactory.Success();
        }

        #endregion
    }
}
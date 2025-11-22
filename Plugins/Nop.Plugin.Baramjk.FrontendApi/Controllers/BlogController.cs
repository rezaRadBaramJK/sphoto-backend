using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Localization;
using Nop.Core.Events;
using Nop.Core.Rss;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Blog;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Blogs;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Blogs;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class BlogController : BaseNopWebApiFrontendController
    {
        #region Ctor

        public BlogController(BlogSettings blogSettings,
            IBlogModelFactory blogModelFactory,
            IBlogService blogService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings)
        {
            _blogSettings = blogSettings;
            _blogModelFactory = blogModelFactory;
            _blogService = blogService;
            _customerActivityService = customerActivityService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
        }

        #endregion

        #region Fields

        private readonly BlogSettings _blogSettings;
        private readonly IBlogModelFactory _blogModelFactory;
        private readonly IBlogService _blogService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Methods

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BlogPostListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> List([FromBody] BlogPagingFilteringModelDto command)
        {
            if (!_blogSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var model =
                await _blogModelFactory.PrepareBlogPostListModelAsync(command.FromDto<BlogPagingFilteringModel>());

            return ApiResponseFactory.Success(model.ToDto<BlogPostListModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BlogPostListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BlogByTag([FromBody] BlogPagingFilteringModelDto command)
        {
            if (!_blogSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var model =
                await _blogModelFactory.PrepareBlogPostListModelAsync(command.FromDto<BlogPagingFilteringModel>());

            return ApiResponseFactory.Success(model.ToDto<BlogPostListModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BlogPostListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BlogByMonth([FromBody] BlogPagingFilteringModelDto command)
        {
            if (!_blogSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var model =
                await _blogModelFactory.PrepareBlogPostListModelAsync(command.FromDto<BlogPagingFilteringModel>());

            return ApiResponseFactory.Success(model.ToDto<BlogPostListModelDto>());
        }

        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ListRss(int languageId)
        {
            if (!_blogSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var feed = new RssFeed(
                $"{await _localizationService.GetLocalizedAsync(await _storeContext.GetCurrentStoreAsync(), x => x.Name)}: Blog",
                "Blog",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            var items = new List<RssItem>();
            var blogPosts =
                await _blogService.GetAllBlogPostsAsync((await _storeContext.GetCurrentStoreAsync()).Id, languageId);
            foreach (var blogPost in blogPosts)
            {
                var blogPostUrl = Url.RouteUrl("BlogPost",
                    new
                    {
                        SeName = await _urlRecordService.GetSeNameAsync(blogPost, blogPost.LanguageId,
                            ensureTwoPublishedLanguages: false)
                    }, _webHelper.GetCurrentRequestProtocol());
                items.Add(new RssItem(blogPost.Title, blogPost.Body, new Uri(blogPostUrl),
                    $"urn:store:{(await _storeContext.GetCurrentStoreAsync()).Id}:blog:post:{blogPost.Id}",
                    blogPost.CreatedOnUtc));
            }

            feed.Items = items;

            return ApiResponseFactory.Success(feed.GetContent());
        }

        [HttpGet("{blogPostId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BlogPostModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBlogPost(int blogPostId)
        {
            if (!_blogSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var blogPost = await _blogService.GetBlogPostByIdAsync(blogPostId);
            if (blogPost == null)
                return ApiResponseFactory.NotFound($"Blog post Id={blogPostId} not found");

            var notAvailable =
                //availability dates
                !_blogService.BlogPostIsAvailable(blogPost) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(blogPost);
            //Check whether the current user has a "Manage blog" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                                 await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog);
            if (notAvailable && !hasAdminAccess)
                return ApiResponseFactory.NotFound(
                    $"Blog post Id={blogPostId} is not available or current user has not a 'Manage blog' permission.");

            var model = new BlogPostModel();
            await _blogModelFactory.PrepareBlogPostModelAsync(model, blogPost, true);

            return ApiResponseFactory.Success(model.ToDto<BlogPostModelDto>());
        }

        [HttpPost("{blogPostId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BlogCommentAdd([FromBody] BlogPostModelDto model, int blogPostId)
        {
            if (!_blogSettings.Enabled)
                return ApiResponseFactory.BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var blogPost = await _blogService.GetBlogPostByIdAsync(blogPostId);
            if (blogPost == null)
                return ApiResponseFactory.NotFound($"Blog post Id={blogPostId} not found");

            if (!blogPost.AllowComments)
                return ApiResponseFactory.BadRequest($"The setting {nameof(blogPost.AllowComments)} is not true.");

            var comment = new BlogComment
            {
                BlogPostId = blogPost.Id,
                CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                CommentText = model.AddNewComment.CommentText,
                IsApproved = !_blogSettings.BlogCommentsMustBeApproved,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _blogService.InsertBlogCommentAsync(comment);

            //notify a store owner
            if (_blogSettings.NotifyAboutNewBlogComments)
                await _workflowMessageService.SendBlogCommentNotificationMessageAsync(comment,
                    _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddBlogComment",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddBlogComment"), comment);

            //raise event
            if (comment.IsApproved)
                await _eventPublisher.PublishAsync(new BlogCommentApprovedEvent(comment));

            return ApiResponseFactory.Success();
        }

        #endregion
    }
}
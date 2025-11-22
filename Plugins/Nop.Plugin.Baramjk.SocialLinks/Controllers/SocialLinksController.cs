using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.SocialLinks.Domain;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.SocialLinks.Controllers
{
    public class SocialLinksController : BaseBaramjkPluginController
    {
        private readonly IRepository<SocialLink> _repository;
        private readonly IPictureService _pictureService;

        public SocialLinksController(IRepository<SocialLink> repository, IPictureService pictureService)
        {
            _repository = repository;
            _pictureService = pictureService;
        }
        [AllowAnonymous]
        [HttpGet("/FrontendApi/SocialLinks/Links")]
        public async Task<IActionResult> GetSocialLinks()
        {
            var items = await _repository.Table
                .Where(x => x.Category == SocialLinkCategory.SocialMedia)
                .OrderBy(x => x.Priority)
                .ToListAsync();
            var dto = items.SelectAwait(async x => new SocialLinkDto
            {
                Id = x.Id,
                Link = x.Link,
                Category = x.Category,
                ImageId = x.ImageId,
                ImageUrl = await _pictureService.GetPictureUrlAsync(x.ImageId),
                Name = x.Name,
                Priority = x.Priority,
                ShowInWidget = x.ShowInWidget,
                ShowInFooter = x.ShowInFooter,
                ShowInProductDetails = x.ShowInProductDetails
            }).ToEnumerable();
            return ApiResponseFactory.Success(dto);
        }
    }
}
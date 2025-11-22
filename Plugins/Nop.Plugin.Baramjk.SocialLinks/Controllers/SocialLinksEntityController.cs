using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;
using Nop.Plugin.Baramjk.SocialLinks.Domain;
using Nop.Services.Media;
using Nop.Web.Framework;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Baramjk.SocialLinks.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class SocialLinksEntityController : BaseBaramjkPluginController
    {
        private readonly IRepository<SocialLink> _repository;
        private readonly IPictureService _pictureService;

        public SocialLinksEntityController(IRepository<SocialLink> repository, IPictureService pictureService)
        {
            _repository = repository;
            _pictureService = pictureService;
        }

        [HttpGet("Admin/SocialLink/SocialLinksEntity/CreateSocialMedia")]
        public async Task<IActionResult> CreateSocialMedia()
        {
            ViewBag.category = SocialLinkCategory.SocialMedia;
            return View("SocialLink/Create.cshtml", new SocialLink());
        }

        [HttpGet("Admin/SocialLink/SocialLinksEntity/ListSocialMedia")]
        public async Task<IActionResult> ListSocialMedia(SocialLinkSearchModel searchModel)
        {
            return View("SocialLink/ListSocialMedia.cshtml", new SocialLinkSearchModel());
        }

        private async Task<SocialLinkList> GetList(SocialLinkSearchModel searchModel, SocialLinkCategory category)
        {
            var items = await _repository.Table
                .Where(x => x.Category == category)
                .OrderBy(x => x.Priority)
                .ToPagedListAsync(searchModel.Page-1, searchModel.PageSize);

            return await new SocialLinkList().PrepareToGridAsync(searchModel, items,
                () =>
                {
                    return items.SelectAwait(async x => new SocialLinkDto
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
                    });
                });
        }

        [HttpPost("Admin/SocialLink/SocialLinksEntity/ListSocialMediaData")]
        public async Task<IActionResult> ListSocialMediaData(SocialLinkSearchModel searchModel)
        {
            ViewBag.category = SocialLinkCategory.SocialMedia;
            var dto = await GetList(searchModel, SocialLinkCategory.SocialMedia);
            return Json(dto);
        }

        [HttpGet("Admin/SocialLink/SocialLinksEntity/ListPaymentMethod")]
        public async Task<IActionResult> ListPaymentMethod(SocialLinkSearchModel searchModel)
        {
            return View("SocialLink/ListPaymentMethod.cshtml", new SocialLinkSearchModel());
        }

        [HttpPost("Admin/SocialLink/SocialLinksEntity/ListPaymentMethodData")]
        public async Task<IActionResult> ListPaymentMethodData(SocialLinkSearchModel searchModel)
        {
            ViewBag.category = SocialLinkCategory.PaymentMethod;
            var dto = await GetList(searchModel, SocialLinkCategory.PaymentMethod);
            return Json(dto);
        }

        [HttpGet("Admin/SocialLink/SocialLinksEntity/CreatePaymentMethod")]
        public async Task<IActionResult> CreatePaymentMethod()
        {
            ViewBag.category = SocialLinkCategory.PaymentMethod;
            return View("SocialLink/Create.cshtml", new SocialLink());
        }

        public async Task<IActionResult> Store(SocialLink model)
        {
            await _repository.InsertAsync(model);
            if (model.Category == SocialLinkCategory.PaymentMethod)
            {
                return RedirectToAction("ListPaymentMethod");
            }
            else
            {
                return RedirectToAction("ListSocialMedia");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var sc = await _repository.GetByIdAsync(id);
            await _repository.DeleteAsync(sc);
            return Json(new { Result = true });
        }

        [HttpGet("Admin/SocialLink/SocialLinksEntity/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var sc = await _repository.GetByIdAsync(id);
            ViewBag.category = sc.Category;
            return View("SocialLink/Edit.cshtml", sc);
        }

        [HttpPost("Admin/SocialLink/SocialLinksEntity/Edit/{id}")]
        public async Task<IActionResult> Update(int id, SocialLink model)
        {
            var sc = await _repository.GetByIdAsync(id);
            sc.Name = model.Name;
            sc.Link = model.Link;
            sc.Priority = model.Priority;
            sc.ImageId = model.ImageId;
            sc.ShowInProductDetails = model.ShowInProductDetails;
            sc.SocialSharePrefix = model.SocialSharePrefix;
            await _repository.UpdateAsync(sc);
            if (model.Category == SocialLinkCategory.PaymentMethod)
            {
                return RedirectToAction("ListPaymentMethod");
            }
            else
            {
                return RedirectToAction("ListSocialMedia");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Plugin.Baramjk.Framework.Factories;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Models.Vendors;
using Nop.Plugin.Baramjk.Framework.Services.Dispatches;
using Nop.Plugin.Baramjk.Framework.Services.Products;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors;
using Nop.Plugin.Baramjk.FrontendApi.Factories;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Services;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.CustomerVendor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Vendors;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    public class VendorController : BaseNopWebApiFrontendController
    {
        private readonly ICustomerService _customerService;
        private readonly FavoriteVendorService _favoriteVendorService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly VendorFactory _vendorFactory;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IVendorRegisterService _vendorRegisterService;
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorDtoFactory _vendorDtoFactory;
        private readonly IDispatcherService _dispatcherService;
        private readonly IWebApiVendorModelFactory _webApiVendorModelFactory;

        public VendorController(
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IVendorModelFactory vendorModelFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            VendorSettings vendorSettings, VendorFactory vendorFactory, FavoriteVendorService favoriteVendorService,
            IVendorRegisterService vendorRegisterService, IVendorDtoFactory vendorDtoFactory,
            IDispatcherService dispatcherService, IWebApiVendorModelFactory webApiVendorModelFactory)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _vendorModelFactory = vendorModelFactory;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _vendorSettings = vendorSettings;
            _vendorFactory = vendorFactory;
            _favoriteVendorService = favoriteVendorService;
            _vendorRegisterService = vendorRegisterService;
            _vendorDtoFactory = vendorDtoFactory;
            _dispatcherService = dispatcherService;
            _webApiVendorModelFactory = webApiVendorModelFactory;
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<VendorModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetVendorByCategoryIdAsync(int categoryId, int count)
        {
            var vendorModels = await _vendorFactory.PrepareVendorModelsByCategoryIdAsync(categoryId, count);
            return ApiResponseFactory.Success(vendorModels);
        }

        [HttpGet("{vendorId}")]
        [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetVendor(int vendorId)
        {
            var model = await _vendorDtoFactory.PrepareVendorDtoAsync(vendorId);
            await _dispatcherService.PublishAsync("VendorDto", model);
            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VendorModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAllVendorsAsync()
        {
            var vendorModels = await _vendorFactory.GetAllVendorsAsync(new SearchVendorRequest());
            return ApiResponseFactory.Success(vendorModels);
        }

        [HttpPost]
        [ProducesResponseType(typeof(List<VendorModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> SearchVendorsAsync([FromBody] SearchVendorRequest request)
        {
            var vendorModels = await _vendorFactory.GetAllVendorsAsync(request);

            return ApiResponseFactory.Success(vendorModels);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AddVendorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> AddVendorAsync([FromBody] AddVendorModel model)
        {
            var addVendorResponse = await _vendorRegisterService.AddVendorAsync(model);
            return ApiResponseFactory.Success(addVendorResponse);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AddVendorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> UpdateCustomerToVendorAsync(
            [FromBody] UpdateCustomerToVendorModel model)
        {
            model.Customer = await _workContext.GetCurrentCustomerAsync();
            var addVendorResponse = await _vendorRegisterService.UpdateCustomerToVendorAsync(model);
            return ApiResponseFactory.Success(addVendorResponse);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApplyVendorModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ApplyVendor()
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return ApiResponseFactory.BadRequest();

            var model = new ApplyVendorModel();
            model = await _vendorModelFactory.PrepareApplyVendorModelAsync(model, true, false, null);

            return ApiResponseFactory.Success(model.ToDto<ApplyVendorModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApplyVendorModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ApplyVendorSubmit([FromBody] ApplyVendorRequest request,
            [FromQuery] [Required] string contentType)
        {
            if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                return ApiResponseFactory.BadRequest(
                    $"The setting {nameof(_vendorSettings.AllowCustomersToApplyForVendorAccount)} is not enabled.");

            if (await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                return ApiResponseFactory.BadRequest(
                    await _localizationService.GetResourceAsync("Vendors.ApplyAccount.IsAdmin"));

            var pictureId = request.PictureId;

            //vendor attributes
            var vendorAttributesXml = await ParseVendorAttributesAsync(request.Form);
            (await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml)).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            var vendorModel = request.Model.FromDto<ApplyVendorModel>();

            var description =
                HtmlHelper.FormatText(vendorModel.Description, false, false, true, false, false, false);
            //disabled by default
            var vendor = new Vendor
            {
                Name = vendorModel.Name,
                Email = vendorModel.Email,
                //some default settings
                PageSize = 6,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions,
                PictureId = pictureId,
                Description = description
            };
            await _vendorService.InsertVendorAsync(vendor);
            //search engine name (the same as vendor name)
            var seName = await _urlRecordService.ValidateSeNameAsync(vendor, vendor.Name, vendor.Name, true);
            await _urlRecordService.SaveSlugAsync(vendor, seName, 0);

            //associate to the current customer
            //but a store owner will have to manually add this customer role to "Vendors" role
            //if he wants to grant access to admin area
            (await _workContext.GetCurrentCustomerAsync()).VendorId = vendor.Id;
            await _customerService.UpdateCustomerAsync(await _workContext.GetCurrentCustomerAsync());

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(vendor);

            //save vendor attributes
            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes,
                vendorAttributesXml);

            //notify store owner here (email)
            await _workflowMessageService.SendNewVendorAccountApplyStoreOwnerNotificationAsync(
                await _workContext.GetCurrentCustomerAsync(),
                vendor, _localizationSettings.DefaultAdminLanguageId);

            vendorModel.DisableFormInput = true;
            vendorModel.Result = await _localizationService.GetResourceAsync("Vendors.ApplyAccount.Submitted");
            var applyVendorModelDto = vendorModel.ToDto<ApplyVendorModelDto>();
            applyVendorModelDto.Id = vendor.Id;
            return ApiResponseFactory.Success(applyVendorModelDto);
        }

        [HttpGet]
        [ProducesResponseType(typeof(VendorInfoModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Info()
        {
            if (await _workContext.GetCurrentVendorAsync() == null || !_vendorSettings.AllowVendorsToEditInfo)
                return ApiResponseFactory.BadRequest();

            var model = new VendorInfoModelDto();
            model = await _webApiVendorModelFactory.PrepareVendorInfoModelAsync(model, false);

            return ApiResponseFactory.Success(model);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VendorAttributeModelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> VendorAttributes()
        {
            var attributeModelDtos = new List<VendorAttributeModelDto>();
            var attributes = await _vendorAttributeService.GetAllVendorAttributesAsync();
            foreach (var attribute in attributes)
            {
                var values = await _vendorAttributeService.GetVendorAttributeValuesAsync(attribute.Id);
                var valueDtos = values.Select(item => item.ToDto<VendorAttributeValueModelDto>()).ToList();

                var modelDto = attribute.ToDto<VendorAttributeModelDto>();
                modelDto.Values = valueDtos;

                attributeModelDtos.Add(modelDto);
            }

            return ApiResponseFactory.Success(attributeModelDtos);
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(VendorInfoModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Info([FromBody] InfoRequest request,
            [FromQuery] [Required] string contentType)
        {
            if (await _workContext.GetCurrentVendorAsync() == null || !_vendorSettings.AllowVendorsToEditInfo)
                return ApiResponseFactory.NotFound(
                    "The current vendor is null or vendor can not edit information about itself.");

            Picture picture = null;

            if (request.PictureBinary.Any())
                try
                {
                    picture = await _pictureService.InsertPictureAsync(request.PictureBinary, contentType, null);
                }
                catch (Exception)
                {
                    return ApiResponseFactory.BadRequest(new List<string>
                    {
                        await _localizationService.GetResourceAsync("Account.VendorInfo.Picture.ErrorMessage")
                    });
                }

            var vendor = await _workContext.GetCurrentVendorAsync();
            var prevPicture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);

            //vendor attributes
            var vendorAttributesXml = await ParseVendorAttributesAsync(request.Form);
            var attrributeWarnings = await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml);

            if (attrributeWarnings.Any())
                return ApiResponseFactory.BadRequest(attrributeWarnings);

            var vendorModel = request.Model.FromDto<VendorInfoModel>();

            var description =
                HtmlHelper.FormatText(vendorModel.Description, false, false, true, false, false, false);

            vendor.Name = vendorModel.Name;
            vendor.Email = vendorModel.Email;
            vendor.Description = description;

            if (picture != null)
            {
                vendor.PictureId = picture.Id;

                if (prevPicture != null)
                    await _pictureService.DeletePictureAsync(prevPicture);
            }

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(vendor);

            await _vendorService.UpdateVendorAsync(vendor);

            //save vendor attributes
            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes,
                vendorAttributesXml);

            //notifications
            if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                await _workflowMessageService.SendVendorInformationChangeNotificationAsync(vendor,
                    _localizationSettings.DefaultAdminLanguageId);

            return ApiResponseFactory.Success(vendorModel.ToDto<VendorInfoModelDto>());
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> RemovePicture()
        {
            if (await _workContext.GetCurrentVendorAsync() == null || !_vendorSettings.AllowVendorsToEditInfo)
                return ApiResponseFactory.BadRequest();

            var vendor = await _workContext.GetCurrentVendorAsync();
            var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);

            if (picture != null)
                await _pictureService.DeletePictureAsync(picture);

            vendor.PictureId = 0;
            await _vendorService.UpdateVendorAsync(vendor);

            //notifications
            if (_vendorSettings.NotifyStoreOwnerAboutVendorInformationChange)
                await _workflowMessageService.SendVendorInformationChangeNotificationAsync(vendor,
                    _localizationSettings.DefaultAdminLanguageId);

            return ApiResponseFactory.Success();
        }

        #region Utilities

        protected virtual async Task UpdatePictureSeoNamesAsync(Vendor vendor)
        {
            var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
            if (picture != null)
                await _pictureService.SetSeoFilenameAsync(picture.Id,
                    await _pictureService.GetPictureSeNameAsync(vendor.Name));
        }

        protected virtual async Task<string> ParseVendorAttributesAsync(IDictionary<string, string> form)
        {
            var attributesXml = string.Empty;

            if (form == null)
                return attributesXml;

            var attributes = await _vendorAttributeService.GetAllVendorAttributesAsync();
            foreach (var attribute in attributes)
            {
                var controlId = $"{NopVendorDefaults.VendorAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                        break;
                    case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                            foreach (var item in cblAttributes.Split(new[] { ',' },
                                         StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                    }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _vendorAttributeService.GetVendorAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                                     .Where(v => v.IsPreSelected)
                                     .Select(v => v.Id)
                                     .ToList())
                            attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml, attribute,
                                selectedAttributeId.ToString());
                    }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.Trim();
                            attributesXml = _vendorAttributeParser.AddVendorAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }

                        break;
                }
            }

            return attributesXml;
        }

        #endregion
    }
}
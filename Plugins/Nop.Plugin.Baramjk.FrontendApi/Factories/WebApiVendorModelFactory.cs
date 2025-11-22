using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Vendors;

namespace Nop.Plugin.Baramjk.FrontendApi.Factories
{
    public class WebApiVendorModelFactory : IWebApiVendorModelFactory
    {
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        
        public WebApiVendorModelFactory(IWorkContext workContext, IPictureService pictureService, MediaSettings mediaSettings, IGenericAttributeService genericAttributeService, IVendorAttributeService vendorAttributeService, ILocalizationService localizationService, IVendorAttributeParser vendorAttributeParser)
        {
            _workContext = workContext;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _genericAttributeService = genericAttributeService;
            _vendorAttributeService = vendorAttributeService;
            _localizationService = localizationService;
            _vendorAttributeParser = vendorAttributeParser;
        }

        public async Task<VendorInfoModelDto> PrepareVendorInfoModelAsync(VendorInfoModelDto model, bool excludeProperties,
            string overriddenVendorAttributesXml = "")
        {
            
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var vendor = await _workContext.GetCurrentVendorAsync();
            model.Id = vendor.Id;
            if (!excludeProperties)
            {
                model.Description = vendor.Description;
                model.Email = vendor.Email;
                model.Name = vendor.Name;
                model.PictureId = vendor.PictureId;
            }

            var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
            var pictureSize = _mediaSettings.AvatarPictureSize;
            (model.PictureUrl, _) = picture != null ? await _pictureService.GetPictureUrlAsync(picture,pictureSize) : (string.Empty, null);

            //vendor attributes
            if (string.IsNullOrEmpty(overriddenVendorAttributesXml))
                overriddenVendorAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(vendor, NopVendorDefaults.VendorAttributes);
            model.VendorAttributes = await PrepareVendorAttributesAsync(overriddenVendorAttributesXml);

            return model;
        }
        
        
        /// <summary>
        /// Prepare vendor attribute models
        /// </summary>
        /// <param name="vendorAttributesXml">Vendor attributes in XML format</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the vendor attribute model
        /// </returns>
        protected virtual async Task<IList<VendorAttributeModelDto>> PrepareVendorAttributesAsync(string vendorAttributesXml)
        {
            var result = new List<VendorAttributeModelDto>();

            var vendorAttributes = await _vendorAttributeService.GetAllVendorAttributesAsync();
            foreach (var attribute in vendorAttributes)
            {
                var attributeModel = new VendorAttributeModelDto
                {
                    Id = attribute.Id,
                    Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _vendorAttributeService.GetVendorAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new VendorAttributeValueModelDto
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(vendorAttributesXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = await _vendorAttributeParser.ParseVendorAttributeValuesAsync(vendorAttributesXml);
                                foreach (var attributeValue in selectedValues)
                                    foreach (var item in attributeModel.Values)
                                        if (attributeValue.Id == item.Id)
                                            item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!string.IsNullOrEmpty(vendorAttributesXml))
                            {
                                var enteredText = _vendorAttributeParser.ParseValues(vendorAttributesXml, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //not supported attribute control types
                        break;
                }

                result.Add(attributeModel);
            }

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.AddProductServices;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class AddProductService : IAddProductService
    {
        private readonly ICategoryService _categoryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IProductTagService _productTagService;

        public AddProductService(IProductService productService, ICategoryService categoryService,
            IPictureService pictureService, IWorkContext workContext, IGenericAttributeService genericAttributeService,
            ISpecificationAttributeService specificationAttributeService, IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService, IProductTagService productTagService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _pictureService = pictureService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _specificationAttributeService = specificationAttributeService;
            _manufacturerService = manufacturerService;
            _productAttributeService = productAttributeService;
            _productTagService = productTagService;
        }

        public async Task<AddProductResponse> AddProductAsync(ProductDataModel dataModel)
        {
            var validate = await ValidateAsync(dataModel);
            if (validate != null)
                return new AddProductResponse { ErrorMessage = validate };

            var vendorId = (await _workContext.GetCurrentVendorAsync())?.Id;

            var product = new Product
            {
                ProductType = ProductType.SimpleProduct,
                ShortDescription = dataModel.FullDescription,
                FullDescription = dataModel.FullDescription,
                Price = dataModel.PricePerQuantity,
                VendorId = vendorId ?? 0,
                StockQuantity = dataModel.Quantity,
                Name = dataModel.ProductName,
                Published = true,
                VisibleIndividually = true,
                Sku = dataModel.Sku,
                AvailableStartDateTimeUtc = dataModel.AvailableStartDateTimeUtc,
                AvailableEndDateTimeUtc = dataModel.AvailableEndDateTimeUtc,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            await _productService.InsertProductAsync(product);
            await AddProductCategoriesAsync(product.Id, dataModel);
            await AddProductPicturesAsync(product.Id, dataModel);
            await InsertProductManufacturerAsync(dataModel, product);
            await InsertGenericAttributesAsync(dataModel, product);
            await InsertProductSpecificationAttributesAsync(product.Id, dataModel.SpecificationAttributes);
            await InsertAttributesAsync(product.Id, dataModel.Attributes);
            await UpdateProductTagsAsync(product, dataModel.Tags);

            var addProductResponse = new AddProductResponse { ProductId = product.Id, ErrorMessage = null };
            return addProductResponse;
        }


        public async Task<AddProductResponse> EditProductAsync(int id, ProductDataModel dataModel)
        {
            var validate = await ValidateAsync(dataModel);
            if (validate != null)
                return new AddProductResponse { ErrorMessage = validate };

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return new AddProductResponse { ErrorMessage = "product not found" };
            var vendor = await _workContext.GetCurrentVendorAsync();
            if (product.VendorId != vendor?.Id)
                return new AddProductResponse { ErrorMessage = "you cant edit this product" };

            product.ShortDescription = dataModel.FullDescription;
            product.FullDescription = dataModel.FullDescription;
            product.Price = dataModel.PricePerQuantity;
            product.VendorId = vendor.Id;
            product.StockQuantity = dataModel.Quantity;
            product.Name = dataModel.ProductName;
            product.Sku = dataModel.Sku;
            product.AvailableStartDateTimeUtc = dataModel.AvailableStartDateTimeUtc;
            product.AvailableEndDateTimeUtc = dataModel.AvailableEndDateTimeUtc;
            product.UpdatedOnUtc = DateTime.UtcNow;

            //  product.Published = true; todo:

            if (dataModel.PictureIds != null && dataModel.PictureIds.Length > 0)
            {
                var pictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
                foreach (var picture in pictures)
                    await _productService.DeleteProductPictureAsync(picture);
            }

            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
            foreach (var productCategory in productCategories)
                await _categoryService.DeleteProductCategoryAsync(productCategory);

            var manufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
            foreach (var manufacturer in manufacturers)
                await _manufacturerService.DeleteProductManufacturerAsync(manufacturer);

            var specs = await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id);
            foreach (var attribute in specs)
                await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(attribute);

            var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in attributes)
                await _productAttributeService.DeleteProductAttributeMappingAsync(attribute);

            await _productService.UpdateProductAsync(product);
            await AddProductPicturesAsync(product.Id, dataModel);
            await AddProductCategoriesAsync(product.Id, dataModel);
            await InsertProductManufacturerAsync(dataModel, product);
            await InsertGenericAttributesAsync(dataModel, product);
            await InsertProductSpecificationAttributesAsync(product.Id, dataModel.SpecificationAttributes);
            await InsertAttributesAsync(product.Id, dataModel.Attributes);
            await UpdateProductTagsAsync(product, dataModel.Tags);

            var addProductResponse = new AddProductResponse { ProductId = product.Id, ErrorMessage = null };
            return addProductResponse;
        }

#pragma warning disable CS1998
        private async Task<string> ValidateAsync(ProductDataModel dataModel)
#pragma warning restore CS1998
        {
            return null;
            /*  if (await _workContext.GetCurrentCustomerAsync() == null)
                              return "Vendor not found";
             if ((dataModel.PictureIds == null || dataModel.PictureIds.Length < 1))
                return "No image has been uploaded";

            if (dataModel.CategoryIds == null || dataModel.CategoryIds.Length < 1)
                return "category its not sent correct";

            /*if (Enum.IsDefined(typeof(Condition), model.Condition))
                return "The Condition was not sent correctly";

            if (string.IsNullOrEmpty(dataModel.FullDescription))
                return "ProductDescription is empty";

            if (string.IsNullOrEmpty(dataModel.ProductName))
                return "ProductName is empty";

              if (dataModel.Quantity < 1)
                  return "The Quantity was not sent correctly";
  
              if (dataModel.PricePerQuantity < 00.00001M)
                  return "The PricePerQuantity was not sent correctly";
  
            return null;*/
        }

        private async Task UpdateProductTagsAsync(Product product, string[] tags)
        {
            if (tags.IsEmptyOrNull())
                return;

            await _productTagService.UpdateProductTagsAsync(product, tags);
        }

        private async Task InsertGenericAttributesAsync(ProductDataModel dataModel, Product product)
        {
            if (dataModel.GenericAttributes == null)
                return;

            foreach (var genericAttribute in dataModel.GenericAttributes)
                await _genericAttributeService.SaveAttributeAsync(product, genericAttribute.Key,
                    genericAttribute.Value);
        }

        private async Task InsertProductManufacturerAsync(ProductDataModel dataModel, Product product)
        {
            if (dataModel.BrandId == null)
                return;

            var productManufacturer = new ProductManufacturer
            {
                ManufacturerId = dataModel.BrandId.Value,
                ProductId = product.Id
            };

            await _manufacturerService.InsertProductManufacturerAsync(productManufacturer);
        }

        private async Task InsertProductSpecificationAttributesAsync(int productId,
            List<ProductSpecificationAttribute> specificationAttributes)
        {
            if (specificationAttributes == null)
                return;

            foreach (var attribute in specificationAttributes)
            {
                attribute.ProductId = productId;
                await _specificationAttributeService.InsertProductSpecificationAttributeAsync(attribute);
            }
        }

        private async Task InsertAttributesAsync(int productId, List<ProductAttributeMappingModel> attributes)
        {
            if (attributes == null)
                return;

            foreach (var attribute in attributes)
            {
                attribute.ProductId = productId;
                var productAttributeMapping = new ProductAttributeMapping
                {
                    ProductId = attribute.ProductId,
                    ProductAttributeId = attribute.ProductAttributeId,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlTypeId = attribute.AttributeControlTypeId,
                    DisplayOrder = attribute.DisplayOrder,
                    ValidationMinLength = attribute.ValidationMinLength,
                    ValidationMaxLength = attribute.ValidationMaxLength,
                    ValidationFileAllowedExtensions = attribute.ValidationFileAllowedExtensions,
                    ValidationFileMaximumSize = attribute.ValidationFileMaximumSize,
                    DefaultValue = attribute.DefaultValue,
                    ConditionAttributeXml = attribute.ConditionAttributeXml
                };
                await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMapping);

                if (attribute.Values == null)
                    continue;

                foreach (var valueModel in attribute.Values)
                {
                    var productAttributeValue = new ProductAttributeValue
                    {
                        ProductAttributeMappingId = productAttributeMapping.Id,
                        AttributeValueTypeId = valueModel.AttributeValueTypeId,
                        AssociatedProductId = valueModel.AssociatedProductId,
                        Name = valueModel.Name,
                        ColorSquaresRgb = valueModel.ColorSquaresRgb,
                        ImageSquaresPictureId = valueModel.ImageSquaresPictureId,
                        PriceAdjustment = valueModel.PriceAdjustment,
                        PriceAdjustmentUsePercentage = valueModel.PriceAdjustmentUsePercentage,
                        WeightAdjustment = valueModel.WeightAdjustment,
                        Cost = valueModel.Cost,
                        CustomerEntersQty = valueModel.CustomerEntersQty,
                        Quantity = valueModel.Quantity,
                        IsPreSelected = valueModel.IsPreSelected,
                        DisplayOrder = valueModel.DisplayOrder,
                        PictureId = valueModel.PictureId
                    };
                    await _productAttributeService.InsertProductAttributeValueAsync(productAttributeValue);
                }
            }
        }

        private async Task AddProductPicturesAsync(int productId, ProductDataModel dataModel)
        {
            var displayOrder = 1;

            if (dataModel.PictureIds != null)
                foreach (var pictureId in dataModel.PictureIds)
                    await _productService.InsertProductPictureAsync(new ProductPicture
                    {
                        PictureId = pictureId,
                        ProductId = productId,
                        DisplayOrder = displayOrder++
                    });
        }

        private async Task AddProductCategoriesAsync(int productId, ProductDataModel dataModel)
        {
            foreach (var categoryId in dataModel.CategoryIds)
                await _categoryService.InsertProductCategoryAsync(new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = categoryId,
                    DisplayOrder = 1
                });
        }
    }
}
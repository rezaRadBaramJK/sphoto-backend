using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Extensions;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Factories
{
    public class SupplierAdminFactory
    {
        private readonly SupplierService _supplierService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public SupplierAdminFactory(
            SupplierService supplierService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _supplierService = supplierService;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        public async Task<SupplierListModel> PrepareListModelAsync(SupplierSearchModel searchModel)
        {
            var pageSize = searchModel.PageSize;
            var pageIndex = searchModel.Page - 1;
            var suppliers = await _supplierService.GetAsync(pageIndex, pageSize);
            return await new SupplierListModel().PrepareToGridAsync(searchModel, suppliers, () =>
            {
                return suppliers.SelectAwait(async sp =>
                {
                    var viewModel = sp.Map<SupplierViewModel>();
                    viewModel.Name = await _localizationService.GetLocalizedAsync(sp, s => s.Name);
                    return viewModel;
                });
            });
        }

        public async Task<SupplierViewModel> PrepareViewModelAsync(Supplier supplier = null)
        {
            SupplierViewModel viewModel = null;
            if (supplier != null)
            {
                viewModel = supplier.Map<SupplierViewModel>();
            }
            viewModel ??= new SupplierViewModel();
            viewModel.Locales = await PrepareLocalizationsAsync(supplier);

            return viewModel;
        }

        private async Task<IList<SupplierLocalizationModel>> PrepareLocalizationsAsync(Supplier supplier)
        {
            var availableLanguages = await _languageService.GetAllLanguagesAsync(true);
            return await availableLanguages.SelectAwait(async lang => new SupplierLocalizationModel
            {
                LanguageId = lang.Id,
                Name = supplier == null ? string.Empty : await _localizationService.GetLocalizedAsync(supplier, s => s.Name, lang.Id, false)
            }).ToListAsync();
        }

        public async Task<ProductSupplierViewModel> PrepareProductSupplierAsync(int productId)
        {
            var viewModel = new ProductSupplierViewModel
            {
                ProductId = productId,
                AvailableSuppliers = await PrepareAvailableSuppliersAsync()
            };

            var productSupplierMapping = await _supplierService.GetProductSupplierMappingByProductId(productId);

            if (productSupplierMapping != null)
            {
                viewModel.SupplierId = productSupplierMapping.SupplierId;
            }
            return viewModel;
        }

        public async Task<IList<SelectListItem>> PrepareAvailableSuppliersAsync()
        {
            var suppliers = await _supplierService.GetAsync();
            var items = await suppliers.SelectAwait(async supplier =>
            {
                var name = await _localizationService.GetLocalizedAsync(supplier, s => s.Name);
                return new SelectListItem
                {
                    Text = $"{name} - {supplier.SupplierCode}",
                    Value = supplier.Id.ToString()
                };
            }).ToListAsync();
            items.Insert(0, new SelectListItem { Text = "Select a Supplier", Value = "0" });
            return items;
        }
    }
}
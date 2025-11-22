using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Wallet.Controllers;
using Nop.Plugin.Baramjk.Wallet.Models.ViewModels.Packages;
using Nop.Plugin.Baramjk.Wallet.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Baramjk.Wallet.Factories
{
    public class PackageAdminFactory
    {
        private readonly WalletPackageService _walletPackageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILanguageService _languageService;

        public PackageAdminFactory(
            WalletPackageService walletPackageService,
            ILocalizationService localizationService, IPriceFormatter priceFormatter, ILanguageService languageService)
        {
            _walletPackageService = walletPackageService;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _languageService = languageService;
        }

        public async Task<AddOrEditPackageViewModel> PrepareEditModelAsync(int id)
        {
            var availableLanguages = await _languageService.GetAllLanguagesAsync(true);

            AddOrEditPackageViewModel model = null;
            var package = await _walletPackageService.GetByIdAsync(id);
            if (package is not null)
            {
                model = new AddOrEditPackageViewModel()
                {
                    Id = package.Id,
                    Name = package.Name,
                    Locales = await availableLanguages.SelectAwait(async language =>
                    {
                        return new PostAddOrEditPackageModelLocalizedModel
                        {
                            LanguageId = language.Id,
                            Name = await _localizationService.GetLocalizedAsync(
                                package,
                                entity => entity.Name,
                                language.Id,
                                false,
                                false)
                        };
                    }).ToListAsync()
                };
            }

            return model;
        }

        public async Task<AddOrEditPackageItemViewModel> PreparePackageItemEditModelAsync(int id)
        {
            var availableLanguages = await _languageService.GetAllLanguagesAsync(true);

            AddOrEditPackageItemViewModel model = null;
            var packageItem = await _walletPackageService.GetWalletItemPackageAsync(id);
            if (packageItem is not null)
            {
                model = new AddOrEditPackageItemViewModel()
                {
                    Id = packageItem.Id,
                    PackageId = packageItem.WalletPackageId,
                    CurrencyCode = packageItem.CurrencyCode,
                    Amount = packageItem.Amount,
                };
            }

            return model;
        }

        public async Task<AddOrEditPackageViewModel> PrepareAddModelAsync()
        {
            var availableLanguages = await _languageService.GetAllLanguagesAsync(true);

            AddOrEditPackageViewModel model = new AddOrEditPackageViewModel()
            {
                Locales = availableLanguages.Select(language => new PostAddOrEditPackageModelLocalizedModel
                {
                    LanguageId = language.Id,
                }).ToList()
            };

            return model;
        }

        public PackageSearchModel PrepareSearchModel()
        {
            return new PackageSearchModel();
        }

        public async Task<PackageListModel> PrepareListModelAsync(PackageSearchModel searchModel)
        {
            var pageIndex = searchModel.Page - 1;
            var packages = await _walletPackageService.GetAsync(pageIndex);
            var packageItems = await _walletPackageService.GetAllPackageItemsAsync();
            return await new PackageListModel().PrepareToGridAsync(searchModel, packages, () =>
            {
                return packages.SelectAwait(async package => new PackageListModelItem
                {
                    Name = await _localizationService.GetLocalizedAsync(package, p => p.Name),
                    Id = package.Id,
                    AmountValues = packageItems
                        .Where(pi => pi.WalletPackageId == package.Id)
                        .Select(p => new PackageAmountValue()
                        {
                            Id = p.Id,
                            Title = $"{p.Amount.ToString("N2")} {p.CurrencyCode}",
                        }).ToList(),
                });
            });
        }
    }
}
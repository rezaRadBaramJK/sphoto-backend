using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.Wallet.Controllers;
using Nop.Plugin.Baramjk.Wallet.Domain;
using Nop.Plugin.Baramjk.Wallet.Models.ViewModels.Packages;
using Nop.Plugin.Baramjk.Wallet.Services.Models;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Plugin.Baramjk.Wallet.Services
{
    public class WalletPackageService
    {
        private readonly ICurrencyService _currencyService;
        private readonly IRepository<WalletItemPackage> _walletPackageItemRepository;
        private readonly IRepository<WalletPackage> _walletPackageRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public WalletPackageService(IRepository<WalletPackage> walletPackageRepository,
            IRepository<WalletItemPackage> walletPackageItemRepository,
            ICurrencyService currencyService, ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _walletPackageRepository = walletPackageRepository;
            _walletPackageItemRepository = walletPackageItemRepository;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }

        public async Task<WalletPackage> AddPackageAsync(string title, List<WalletPackageItemAddModel> itemModels)
        {
            var walletPackage = new WalletPackage
            {
                Name = title
            };

            await _walletPackageRepository.InsertAsync(walletPackage);
            if (itemModels is not null)
            {
                foreach (var itemModel in itemModels)
                    await _walletPackageItemRepository.InsertAsync(new WalletItemPackage
                    {
                        WalletPackageId = walletPackage.Id,
                        Amount = itemModel.Amount,
                        CurrencyCode = itemModel.CurrencyCode.Trim()
                    });
            }

            return walletPackage;
        }

        public async Task<WalletPackage> AddOrEditPackageAsync(AddOrEditPackageViewModel model)
        {
            WalletPackage walletPackage = null;
            if (model.Id.HasValue)
            {
                walletPackage = await _walletPackageRepository.GetByIdAsync(model.Id);
                if (walletPackage != null)
                {
                    walletPackage.Name = model.Name;
                    await _walletPackageRepository.UpdateAsync(walletPackage);
                }
            }
            else
            {
                walletPackage = new WalletPackage()
                {
                    Name = model.Name,
                };
                await _walletPackageRepository.InsertAsync(walletPackage);
            }

            if (walletPackage != null && model.Locales != null)
            {
                foreach (var localized in model.Locales)
                    await _localizedEntityService.SaveLocalizedValueAsync(walletPackage,
                        x => x.Name, localized.Name, localized.LanguageId);
            }

            return walletPackage;
        }


        public async Task<List<PackageModel>> GetPackageModelsAsync()
        {
            var packages = await _walletPackageRepository.Table.ToListAsync();
            var itemPackages = await _walletPackageItemRepository.Table.ToListAsync();
            var currencies = await _currencyService.GetAllCurrenciesAsync();

            var query = from package in packages
                join item in itemPackages on package.Id equals item.WalletPackageId
                join currency in currencies on item.CurrencyCode.Trim() equals currency.CurrencyCode
                select new
                {
                    currency,
                    package,
                    item
                };

            var pakages = await query.ToListAsync();

            var list = await pakages.GroupBy(item => item.package.Id)
                .SelectAwait(async item => new PackageModel
                {
                    Id = item.Key,
                    Name = await _localizationService.GetLocalizedAsync(item.First().package, x => x.Name),
                    Price = decimal.Parse(item.Sum(a => a.item.Amount * (1 / a.currency.Rate)).ToString("N2")),
                    Items = item.Select(a => new WalletItemPackageModel
                        {
                            Amount = decimal.Parse(a.item.Amount.ToString("N2")),
                            CurrencyCode = a.item.CurrencyCode,
                            CurrencyName = a.currency.CurrencyCode
                        }
                    ).ToList()
                }).ToListAsync();

            return list;
        }

        public async Task DeletePackageAsync(int id)
        {
            var package = await GetByIdAsync(id);
            if (package == null)
                return;

            var packageItems = await _walletPackageItemRepository.Table
                .Where(x => x.WalletPackageId == package.Id)
                .ToListAsync();
            if (packageItems.Any())
                foreach (var packageItem in packageItems)
                    await _walletPackageItemRepository.DeleteAsync(packageItem);

            await _walletPackageRepository.DeleteAsync(package);
        }

        public async Task<WalletPackage> GetByIdAsync(int id)
        {
            var package = await _walletPackageRepository.Table.FirstOrDefaultAsync(item => item.Id == id);
            return package;
        }

        public Task<IPagedList<WalletPackage>> GetAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return _walletPackageRepository.Table.ToPagedListAsync(pageIndex, pageSize);
        }

        #region Packages item methods

        public async Task<List<WalletItemPackage>> GetPackageItemsAsync(int packageId)
        {
            return await _walletPackageItemRepository.Table
                .Where(p => p.WalletPackageId == packageId)
                .ToListAsync();
        }

        public async Task<List<WalletItemPackage>> GetAllPackageItemsAsync()
        {
            return await _walletPackageItemRepository.Table
                .ToListAsync();
        }

        public async Task<WalletItemPackage> GetWalletItemPackageAsync(int id)
        {
            return await _walletPackageItemRepository.GetByIdAsync(id);
        }

        public async Task<WalletItemPackage> AddPackageItemAsync(WalletItemPackage packageItem)
        {
            await _walletPackageItemRepository.InsertAsync(packageItem);
            return packageItem;
        }

        public async Task<WalletItemPackage> EditPackageItemAsync(WalletItemPackage packageItem)
        {
            var package = await _walletPackageItemRepository.GetByIdAsync(packageItem.Id);
            if (package is null)
                return null;
            package.CurrencyCode = packageItem.CurrencyCode;
            package.Amount = packageItem.Amount;
            await _walletPackageItemRepository.UpdateAsync(package);
            return package;
        }

        public async Task<bool> IsDuplicatePackageItemByCurrencyCodeAsync(int? id,int packageId, string currencyCode)
        {
            if (id.HasValue)
            {
                return await _walletPackageItemRepository.Table.AnyAsync(x =>
                    x.WalletPackageId==packageId &&
                    x.CurrencyCode == currencyCode && x.Id != id.Value);
            }
            else
            {
                return await _walletPackageItemRepository.Table.AnyAsync(x =>
                    x.WalletPackageId==packageId &&
                    x.CurrencyCode == currencyCode);
            }
        }


        public async Task DeletePackageItemAsync(int id)
        {
            var packageItem = await _walletPackageItemRepository.GetByIdAsync(id);
            if (packageItem == null)
                return;
            await _walletPackageItemRepository.DeleteAsync(packageItem);
        }

        #endregion
    }
}
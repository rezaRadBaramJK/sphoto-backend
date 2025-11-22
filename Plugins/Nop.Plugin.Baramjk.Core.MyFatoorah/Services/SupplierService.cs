using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Core.Domain.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Clients;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Factories;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Plugins;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Tasks;
using Nop.Services.Tasks;
using NUglify.Helpers;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Services
{
    public class SupplierService
    {
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<ProductSupplierMapping> _productSupplierMappingRepository;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly MyFatoorahPaymentClient _myFatoorahPaymentClient;
        private readonly SupplierFactory _supplierFactory;
        private readonly MyFatoorahSettings _myFatoorahSettings;

        public SupplierService(
            IRepository<Supplier> supplierRepository,
            IRepository<ProductSupplierMapping> productSupplierMappingRepository,
            IScheduleTaskService scheduleTaskService,
            MyFatoorahPaymentClient myFatoorahPaymentClient,
            SupplierFactory supplierFactory,
            MyFatoorahSettings myFatoorahSettings)
        {
            _supplierRepository = supplierRepository;
            _productSupplierMappingRepository = productSupplierMappingRepository;
            _scheduleTaskService = scheduleTaskService;
            _myFatoorahPaymentClient = myFatoorahPaymentClient;
            _supplierFactory = supplierFactory;
            _myFatoorahSettings = myFatoorahSettings;
        }


        public Task<IPagedList<Supplier>> GetAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            return _supplierRepository.Table
                .OrderBy(s => s.Name)
                .ToPagedListAsync(pageIndex, pageSize);
        }

        public Task<Supplier> GetByCodeAsync(int code)
        {
            return _supplierRepository.Table.FirstOrDefaultAsync(s => s.SupplierCode == code);
        }

        public Task<Supplier> GetByIdAsync(int supplierId)
        {
            return _supplierRepository.GetByIdAsync(supplierId);
        }

        public Task AddAsync(Supplier supplier)
        {
            return _supplierRepository.InsertAsync(supplier);
        }

        public Task UpdateAsync(Supplier supplier)
        {
            return _supplierRepository.UpdateAsync(supplier);
        }

        public Task DeleteAsync(Supplier supplier)
        {
            return _supplierRepository.DeleteAsync(supplier);
        }

        public async Task DeleteByIdAsync(int supplierId)
        {
            var supplier = await GetByIdAsync(supplierId);
            if (supplier == null)
                return;

            await DeleteAsync(supplier);
        }

        public Task<ProductSupplierMapping> GetProductSupplierMappingByProductId(int productId)
        {
            return _productSupplierMappingRepository.Table.FirstOrDefaultAsync(psm => psm.ProductId == productId);
        }

        public async Task AssignProductAsync(int productId, int supplierId)
        {
            var productSupplierMapping = await _productSupplierMappingRepository.Table.FirstOrDefaultAsync(psm =>
                psm.ProductId == productId);

            if (productSupplierMapping == null)
            {
                productSupplierMapping = new ProductSupplierMapping
                {
                    ProductId = productId,
                    SupplierId = supplierId
                };
                await _productSupplierMappingRepository.InsertAsync(productSupplierMapping);
                return;
            }

            productSupplierMapping.SupplierId = supplierId;
            await _productSupplierMappingRepository.UpdateAsync(productSupplierMapping);
        }

        public async Task<GetSupplierCodeResult> TryGetSupplierTokenAsync(int[] productIds)
        {
            if (productIds == null || productIds.Any() == false)
                return GetSupplierCodeResult.GetFailedResult();

            var query =
                from psm in _productSupplierMappingRepository.Table
                join supplier in _supplierRepository.Table on psm.SupplierId equals supplier.Id
                where productIds.Contains(psm.ProductId)
                select new
                {
                    psm.ProductId,
                    Supplier = supplier
                };

            var result = await query.ToArrayAsync();

            if (result.Length != productIds.Length)
                return GetSupplierCodeResult.GetFailedResult();

            var distinctSuppliers = result
                .Select(x => x.Supplier)
                .DistinctBy(x => x.SupplierCode)
                .ToArray();
            return distinctSuppliers.Length != 1
                ? GetSupplierCodeResult.GetFailedResult()
                : GetSupplierCodeResult.GetSuccessfulResult(distinctSuppliers.First().SupplierCode);
        }

        public Task<Supplier> GetDefaultSupplier()
        {
            return GetByIdAsync(_myFatoorahSettings.DefaultSupplierId);
        }

        public async Task<SupplierProductPair[]> GetProductsSupplierAsync(int[] productIds)
        {
            if (productIds == null || productIds.Any() == false)
                return Array.Empty<SupplierProductPair>();
            
            var query =
                from psm in _productSupplierMappingRepository.Table
                where productIds.Contains(psm.ProductId)
                join supplier in _supplierRepository.Table on psm.SupplierId equals supplier.Id
                select new SupplierProductPair
                {
                    ProductId = psm.ProductId,
                    Supplier = supplier
                };

            return await query.ToArrayAsync();
        }

        public Task<Supplier[]> GetByCodesAsync(int[] supplierCodes)
        {
            return _supplierRepository.Table
                .Where(s => supplierCodes.Contains(s.SupplierCode) && s.Deleted == false)
                .ToArrayAsync();
        }

        public async Task SubmitAsync()
        {
            var typeName =
                $"{typeof(SyncSupplierScheduleTask).FullName}, {typeof(SyncSupplierScheduleTask).Assembly.GetName().Name}";
            var task = await _scheduleTaskService.GetTaskByTypeAsync(typeName);
            if (task == null)
            {
                task = new ScheduleTask
                {
                    Name = "Sync Supplier",
                    Seconds = 86400,
                    Enabled = true,
                    Type = typeName
                };

                await _scheduleTaskService.InsertTaskAsync(task);
            }
        }

        public async Task SyncSuppliersAsync()
        {
            if(_myFatoorahSettings.MyFatoorahUseSandbox)
                return;
            
            var response = await _myFatoorahPaymentClient.GetSuppliersAsync();
            if (response.IsSuccessStatusCode == false)
                return;
            
            var suppliersToSync =  _supplierFactory.PrepareSuppliers(response.Body);
            suppliersToSync = suppliersToSync.ToArray();
            var codes = suppliersToSync.Select(s => s.SupplierCode).ToArray();
            var existSuppliers = await GetByCodesAsync(codes);
            
            //update exists
            var commons = suppliersToSync.Join(
                existSuppliers,
                sToSync => sToSync.SupplierCode,
                existSupplier => existSupplier.SupplierCode,
                (sToSync, existSupplier) =>
                {
                    sToSync.Id = existSupplier.Id;
                    return sToSync;
                }).ToArray();
            
            if(commons.Length > 0)
                await _supplierRepository.UpdateAsync(commons);

            //insert
            var different = existSuppliers.Except(suppliersToSync)
                .Concat(suppliersToSync.Except(existSuppliers))
                .ToArray();
            
            if(different.Length == 0)
                return;

            await _supplierRepository.InsertAsync(different);

        }
    }
}
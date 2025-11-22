using Nop.Plugin.Baramjk.Core.MyFatoorah.Services;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Tasks
{
    public class SyncSupplierScheduleTask: IScheduleTask
    {
        private readonly SupplierService _supplierService;
       

        public SyncSupplierScheduleTask(SupplierService supplierService)
        {
            _supplierService = supplierService;
           
        }

        public Task ExecuteAsync()
        {
            return _supplierService.SyncSuppliersAsync();
        }
    }
}
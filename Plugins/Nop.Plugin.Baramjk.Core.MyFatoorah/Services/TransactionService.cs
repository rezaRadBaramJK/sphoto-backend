using System.Threading.Tasks;
using System.Linq;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Domains;
using Nop.Plugin.Baramjk.Core.MyFatoorah.Models.Suppliers;
using Nop.Plugin.Baramjk.Framework.Domain;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways;
using Nop.Plugin.Baramjk.Framework.Services.Payments.Gateways.Models;

namespace Nop.Plugin.Baramjk.Core.MyFatoorah.Services
{
    public class TransactionService
    {
        private readonly IRepository<GatewayPaymentTranslation> _transactionRepository;
        private readonly ITranslationService _translationService;
        private readonly IRepository<TransactionSupplier> _transactionSupplierRepository;

        public TransactionService(
            IRepository<GatewayPaymentTranslation> transactionRepository,
            ITranslationService translationService,
            IRepository<TransactionSupplier> transactionSupplierRepository)
        {
            _transactionRepository = transactionRepository;
            _translationService = translationService;
            _transactionSupplierRepository = transactionSupplierRepository;
        }

        public async Task<GatewayPaymentTranslation> GetByOrderIdAsync(int orderId)
        {
            return await _transactionRepository.Table
                .FirstOrDefaultAsync(t => t.ConsumerEntityType == nameof(Order) && t.ConsumerEntityId == orderId);
        }
        
        public async Task MarkTransactionAsPaidAsync(int orderId)
        {
            var transaction = await GetByOrderIdAsync(orderId);
            if(transaction == null)
                return;

            if (transaction.Status != GatewayPaymentStatus.Paid)
                transaction.Status = GatewayPaymentStatus.Paid;
            
            if(transaction.ConsumerStatus != ConsumerStatus.Success)
                transaction.ConsumerStatus = ConsumerStatus.Success;

            await _translationService.UpdateAsync(transaction);
        }

        public async Task AddTransactionSupplierAsync(int transactionId, SupplierShare[] suppliers)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
                return;

            var transactionSuppliers = suppliers.Select(s => new TransactionSupplier
            {
                NopSupplierId = s.NopSupplierId,
                InvoiceShare = s.InvoiceShare,
                TransactionId = transactionId
            }).ToArray();

            await _transactionSupplierRepository.InsertAsync(transactionSuppliers);
        }
    }
}
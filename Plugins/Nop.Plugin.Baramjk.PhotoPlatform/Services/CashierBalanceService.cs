using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains.Types;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.CashierBalanceHistory;
using Nop.Services.Logging;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class CashierBalanceService
    {
        private readonly IRepository<CashierBalanceHistory> _cashierBalanceHistoryRepository;
        private readonly ILogger _logger;

        public CashierBalanceService(IRepository<CashierBalanceHistory> cashierBalanceHistoryRepository, ILogger logger)
        {
            _cashierBalanceHistoryRepository = cashierBalanceHistoryRepository;
            _logger = logger;
        }

        private Task InsertAsync(CashierBalanceHistory cashierBalanceHistory)
        {
            return _cashierBalanceHistoryRepository.InsertAsync(cashierBalanceHistory);
        }

        public async Task<bool> CanPerformAsync(CashierBalanceTransactionRequest request)
        {
            if (request.Amount < 0)
                return false;

            // deduction scenarios
            if (request.Type is CashierBalanceHistoryType.AdminDeducted or CashierBalanceHistoryType.SubmittedTicketByCash &&
                await GetBalanceAsync(request.CashierEventId) < request.Amount)
            {
                await _logger.InformationAsync($"CashierEvent: {request.CashierEventId} balance is less that requested amount: {request.Amount}");
                return false;
            }

            return true;
        }

        public async Task<bool> PerformAsync(CashierBalanceTransactionRequest request)
        {
            if (await CanPerformAsync(request) == false)
                return false;


            var cashierBalanceHistory = new CashierBalanceHistory
            {
                Amount = request.Amount,
                CashierEventId = request.CashierEventId,
                Type = request.Type,
                CreatedDateTime = DateTime.Now,
                Note = request.Note,
            };

            await InsertAsync(cashierBalanceHistory);
            return true;
        }

        private static decimal GetSignedAmount(CashierBalanceHistoryType type, decimal amount)
        {
            return type switch
            {
                CashierBalanceHistoryType.AdminDeducted => -amount,
                CashierBalanceHistoryType.SubmittedTicketByCash => -amount,
                CashierBalanceHistoryType.AdminIncreased => amount,
                CashierBalanceHistoryType.RefundedTicketByCash => amount,
                _ => 0
            };
        }

        public async Task<decimal> GetBalanceAsync(int cashierEventId)
        {
            var histories = await _cashierBalanceHistoryRepository.Table
                .Where(cb => cb.CashierEventId == cashierEventId && cb.Deleted == false)
                .ToListAsync();

            if (histories.Any() == false)
                return 0;


            return histories.Sum(h => GetSignedAmount(h.Type, h.Amount));
        }
    }
}
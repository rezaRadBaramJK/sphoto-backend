using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Models.CashierEvents;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class CashierEventService
    {
        private readonly IRepository<CashierEvent> _cashierEventRepository;
        private readonly IRepository<CashierDailyBalance> _cashierDailyBalanceRepository;
        private readonly IRepository<Customer> _customerRepository;

        public CashierEventService(IRepository<CashierEvent> cashierEventRepository,
            IRepository<CashierDailyBalance> cashierDailyBalanceRepository,
            IRepository<Customer> customerRepository)
        {
            _cashierEventRepository = cashierEventRepository;
            _cashierDailyBalanceRepository = cashierDailyBalanceRepository;
            _customerRepository = customerRepository;
        }


        public Task<List<CashierEvent>> GetEventCashiersAsync(int eventId, List<int> cashierIds)
        {
            return _cashierEventRepository.Table.Where(ce => ce.EventId == eventId && cashierIds.Contains(ce.CustomerId) && ce.Deleted == false)
                .ToListAsync();
        }

        public Task<IPagedList<CashierDailyBalanceDetails>> GetEventCashiersDailyBalancesAsync(int eventId, int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var q = from ce in _cashierEventRepository.Table
                join cdb in _cashierDailyBalanceRepository.Table on ce.Id equals cdb.CashierEventId
                join customer in _customerRepository.Table on ce.CustomerId equals customer.Id
                where ce.Deleted == false && ce.EventId == eventId
                select new CashierDailyBalanceDetails { CashierDailyBalance = cdb, Customer = customer };

            return q.ToPagedListAsync(pageIndex, pageSize);
        }

        public Task<CashierDailyBalance> GetCashierDailyBalanceAsync(int cashierEventId, DateTime day)
        {
            return _cashierDailyBalanceRepository.Table
                .Where(x => x.CashierEventId == cashierEventId && x.Day.Date == day.Date)
                .FirstOrDefaultAsync();
        }

        

        public Task<CashierDailyBalance> GetCashierDailyBalanceByIdAsync(int id)
        {
            return _cashierDailyBalanceRepository.GetByIdAsync(id);
        }

        public async Task InsertCashierDailyBalanceAsync(CashierDailyBalance entity)
        {
            await _cashierDailyBalanceRepository.InsertAsync(entity);
        }

        public async Task UpdateCashierDailyBalanceAsync(CashierDailyBalance entity)
        {
            await _cashierDailyBalanceRepository.UpdateAsync(entity);
        }

        public async Task DeleteCashierDailyBalanceAsync(CashierDailyBalance entity)
        {
            await _cashierDailyBalanceRepository.DeleteAsync(entity);
        }

        public Task<List<CashierEvent>> GetEventCashiersAsync(int eventId)
        {
            return _cashierEventRepository.Table
                .Where(ce => ce.EventId == eventId && ce.Deleted == false)
                .ToListAsync();
        }

        public Task<List<Customer>> GetEventCashiersCustomerDataAsync(int eventId)
        {
            var q = from ce in _cashierEventRepository.Table
                join customer in _customerRepository.Table on ce.CustomerId equals customer.Id
                where ce.EventId == eventId && ce.Deleted == false
                select customer;
            return q.ToListAsync();
        }


        public Task InsertAsync(List<CashierEvent> cashierEvents)
        {
            return _cashierEventRepository.InsertAsync(cashierEvents);
        }


        public Task<IPagedList<CashierEvent>> GetAllEventCashierEventsAsync(int eventId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return _cashierEventRepository.Table.Where(ce => ce.EventId == eventId && ce.Deleted == false).ToPagedListAsync(pageIndex, pageSize);
        }


        public Task<CashierEvent> GetByIdAsync(int cashierEventId)
        {
            return _cashierEventRepository.GetByIdAsync(cashierEventId, includeDeleted: false);
        }

        public Task<CashierEvent> GetByCashierIdAsync(int cashierId)
        {
            return _cashierEventRepository.Table.FirstOrDefaultAsync(ce => ce.CustomerId == cashierId && ce.Deleted == false);
        }


        public Task<CashierEvent> GetByCashierIdAndEventIdAsync(int cashierId, int eventId)
        {
            return _cashierEventRepository.Table.FirstOrDefaultAsync(ce =>
                ce.CustomerId == cashierId && ce.EventId == eventId && ce.Deleted == false);
        }

        public Task DeleteAsync(CashierEvent cashierEvent)
        {
            return _cashierEventRepository.DeleteAsync(cashierEvent);
        }


        public Task UpdateAsync(CashierEvent cashierEvent)
        {
            return _cashierEventRepository.UpdateAsync(cashierEvent);
        }

        public Task UpdateAsync(List<CashierEvent> cashierEvents)
        {
            return _cashierEventRepository.UpdateAsync(cashierEvents);
        }

        public Task<bool> HasNotPermittedItemsToRefundAsync(int[] eventIds, int cashierId)
        {
            return _cashierEventRepository.Table.AnyAsync(ce =>
                eventIds.Contains(ce.EventId) && ce.CustomerId == cashierId && ce.Deleted == false && ce.IsRefundPermitted == false);
        }
    }
}
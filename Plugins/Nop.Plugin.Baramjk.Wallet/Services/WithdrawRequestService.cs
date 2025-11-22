using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Baramjk.Framework.Services.Wallets;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Models;
using Nop.Plugin.Baramjk.Framework.Services.Wallets.Types;
using Nop.Plugin.Baramjk.Wallet.Domain;
using Nop.Plugin.Baramjk.Wallet.Models;
using Nop.Plugin.Baramjk.Wallet.Services.Models;

namespace Nop.Plugin.Baramjk.Wallet.Services
{
    public class WithdrawRequestService
    {
        private readonly IRepository<Customer> _repositoryCustomer;
        private readonly IRepository<WithdrawRequest> _repositoryWithdrawRequest;
        private readonly IWorkContext _workContext;
        private readonly IWalletService _walletService;

        public WithdrawRequestService(IRepository<WithdrawRequest> repositoryWithdrawRequest, IWorkContext workContext,
            IRepository<Customer> repositoryCustomer, IWalletService walletService)
        {
            _repositoryWithdrawRequest = repositoryWithdrawRequest;
            _workContext = workContext;
            _repositoryCustomer = repositoryCustomer;
            _walletService = walletService;
        }

        public async Task<WithdrawRequest> AddAsync(WithdrawRequestModel request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == default)
            {
                throw new Exception("Forbidden . customer not passed");
            }

            var balance = await _walletService.GetAvailableAmountAsync(customer.Id, request.CurrencyCode);
            if (request.Amount > balance)
            {
                throw new InsufficientWalletBalanceException();
            }

            if (!await _walletService.CanPerformAsync(new WalletTransactionRequest
                {
                    CustomerId = customer.Id,
                    CurrencyCode = request.CurrencyCode,
                    Amount = request.Amount,
                    Type = WalletHistoryType.Withdrawal,
                    Note = "wallet withdrawal from service"
                }))
            {
                throw new CantPerformWalletTransaction();
            }

            await _walletService.PerformAsync(new WalletTransactionRequest
            {
                CustomerId = customer.Id,
                CurrencyCode = request.CurrencyCode,
                Amount = request.Amount,
                Type = WalletHistoryType.Withdrawal,
                Note = "wallet withdrawal from service"
            });

            var withdrawRequest = new WithdrawRequest
            {
                CurrencyCode = request.CurrencyCode,
                Amount = request.Amount,
                CartNumber = request.CartNumber,
                IBAN = request.IBAN,
                AccountNumber = request.AccountNumber,
                BankName = request.BankName,
                Status = null,
                CustomerId = customer.Id,
                OnDateTime = DateTime.Now
            };

            await _repositoryWithdrawRequest.InsertAsync(withdrawRequest);
            //
            return withdrawRequest;
        }

        public async Task<WithdrawRequest> SetStatusAsync(int id, bool status)
        {
            var withdrawRequest = _repositoryWithdrawRequest.Table.FirstOrDefault(item => item.Id == id);
            if (withdrawRequest == null)
                return null;
            if (withdrawRequest.Status.HasValue)
                return null;
            // no need to check balance because it's already locked
            // var balance =
            //     await _walletService.GetAvailableAmountAsync(withdrawRequest.CustomerId, withdrawRequest.CurrencyCode);
            // if (withdrawRequest.Amount > balance)
            // {
            //     throw new InsufficientWalletBalanceException();
            // }

            await _walletService.PerformAsync(new WalletTransactionRequest
            {
                Amount = withdrawRequest.Amount,
                CustomerId = withdrawRequest.CustomerId,
                CurrencyCode = withdrawRequest.CurrencyCode,
                Type = WalletHistoryType.UnLocking,
                Note = "wallet unlocking from service"
            });
            withdrawRequest.Status = status;
            await _repositoryWithdrawRequest.UpdateAsync(withdrawRequest);
            if (status)
            {
                var withdrawalResult = await _walletService.PerformAsync(new WalletTransactionRequest
                {
                    Amount = withdrawRequest.Amount,
                    CustomerId = withdrawRequest.CustomerId,
                    CurrencyCode = withdrawRequest.CurrencyCode,
                    Type = WalletHistoryType.UnLocking,
                    Note = "wallet unlock from service"
                });
                if (withdrawalResult == false)
                    throw new CantPerformWalletTransaction();
            }

            return withdrawRequest;
        }


        public async Task<List<WithdrawRequest>> ListAsync()
        {
            var withdrawRequests = await _repositoryWithdrawRequest.Table
                .OrderByDescending(item => item.Id)
                .ToListAsync();

            return withdrawRequests;
        }

        public async Task<List<WithdrawRequest>> ListAsync(int customerId, int page = 1, int size = 10,
            string currency = "KWD")
        {
            var withdrawRequests = await _repositoryWithdrawRequest.Table
                .OrderByDescending(item => item.Id)
                .Where(x => x.CustomerId == customerId && x.CurrencyCode == currency)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return withdrawRequests;
        }

        public async Task<List<WithdrawRequestItem>> ListModelAsync()
        {
            var queryable = _repositoryWithdrawRequest.Table.Join(_repositoryCustomer.Table,
                request => request.CustomerId, customer => customer.Id,
                (request, customer) => new WithdrawRequestItem(request.Id, customer.Email, request.CustomerId,
                    request.CurrencyCode,
                    request.Amount, request.CartNumber, request.IBAN, request.AccountNumber, request.BankName,
                    request.Status, request.OnDateTime));

            var withdrawRequests = await queryable.ToListAsync();
            return withdrawRequests;
        }
    }
}
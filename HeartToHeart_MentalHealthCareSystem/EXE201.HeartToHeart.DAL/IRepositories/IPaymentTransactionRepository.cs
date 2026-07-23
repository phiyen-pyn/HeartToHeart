using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IPaymentTransactionRepository
    {
        Task<IEnumerable<PaymentTransaction>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PaymentTransaction?> GetByIdAsync(Guid id);
        Task<PaymentTransaction?> GetByTransactionIdAsync(string transactionId);
        Task<PaymentTransaction?> GetPaymentBySubscriptionIdAsync(Guid subscriptionId);
        Task<PaymentTransaction> AddAsync(PaymentTransaction subscription);
        Task<PaymentTransaction> UpdateAsync(PaymentTransaction subscription);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetTotalCountAsync();
        Task<int> GetTotalCountByStatusAsync(string status);
        Task<int> GetTotalCountByMonthAsync(int year, int month);
        Task<int> GetTotalCountByStatusAndMonthAsync(string status, int year, int month);
        Task<decimal> GetTotalAmountAsync();
        Task<decimal> GetTotalAmountByMonthAsync(int year, int month);
        Task<int> GetTotalByUserIdCountAsync(Guid userId);
    }
}

using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IPaymentTransactionService
    {
        Task<IEnumerable<PaymentTransactionDto>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<PaymentTransactionDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PaymentTransactionDto?> GetByIdAsync(Guid id);
        Task<PaymentTransactionDto?> GetByTransactionIdAsync(string transactionId);
        Task<PaymentTransactionDto?> GetPaymentBySubscriptionIdAsync(Guid subscriptionId);
        Task<PaymentTransactionDto> AddAsync(PaymentTransactionDto dto);
        Task<PaymentTransactionDto> UpdateAsync(Guid id, PaymentTransactionDto dto);
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

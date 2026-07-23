using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EXE201.HeartToHeart.Common.Constants.AppointmentConstants;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentTransaction>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.PaymentTransactions
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.PaymentTransactions
                .Where(p => p.UserId == userId && p.IsActive == true)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PaymentTransaction?> GetByIdAsync(Guid id)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PaymentTransaction?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<PaymentTransaction?> GetPaymentBySubscriptionIdAsync(Guid subscriptionId)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId);
        }

        public async Task<PaymentTransaction> AddAsync(PaymentTransaction subscription)
        {
            _context.PaymentTransactions.Add(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<PaymentTransaction> UpdateAsync(PaymentTransaction subscription)
        {
            _context.PaymentTransactions.Update(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var subscription = await _context.PaymentTransactions.FindAsync(id);
            if (subscription == null)
                return false;
            _context.PaymentTransactions.Remove(subscription);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.PaymentTransactions.CountAsync();
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            return await _context.PaymentTransactions
                .Where(p => p.CreatedAt.Year == year && p.CreatedAt.Month == month)
                .CountAsync();
        }

        public async Task<int> GetTotalCountByStatusAsync(string status)
        {
            return await _context.PaymentTransactions
                .Where(p => p.Status == status && p.IsActive == true)
                .CountAsync();
        }

        public async Task<int> GetTotalCountByStatusAndMonthAsync(string status, int year, int month)
        {
            return await _context.PaymentTransactions
                .Where(p => p.Status == status && p.CreatedAt.Year == year && p.CreatedAt.Month == month && p.IsActive == true)
                .CountAsync();
        }

        public async Task<decimal> GetTotalAmountAsync()
        {
            return await _context.PaymentTransactions
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }
        public Task<decimal> GetTotalAmountByMonthAsync(int year, int month)
        {
            return _context.PaymentTransactions
                .Where(p => p.CreatedAt.Year == year && p.CreatedAt.Month == month && p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }

        public async Task<int> GetTotalByUserIdCountAsync(Guid userId)
        {
            return await _context.PaymentTransactions.Where(p => p.UserId == userId && p.IsActive == true).CountAsync();
        }
    }
}

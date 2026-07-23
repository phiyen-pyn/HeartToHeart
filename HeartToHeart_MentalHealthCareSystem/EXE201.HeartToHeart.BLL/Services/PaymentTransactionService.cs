using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly ILogger<PaymentTransactionService> _logger;

        public PaymentTransactionService(IPaymentTransactionRepository paymentTransactionRepository, ILogger<PaymentTransactionService> logger)
        {
            _paymentTransactionRepository = paymentTransactionRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var convos = await _paymentTransactionRepository.GetAllAsync(page, pageSize);
                return convos.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all PaymentTransactions");
                return Enumerable.Empty<PaymentTransactionDto>();
            }
        }

        public async Task<PaymentTransactionDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var payment = await _paymentTransactionRepository.GetByIdAsync(id);
                if (payment != null && payment.IsActive == false)
                {
                    return null;
                }
                return payment == null ? null : MapToDto(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PaymentTransaction by ID: {PaymentTransactionId}", id);
                return null;
            }
        }

        public async Task<PaymentTransactionDto?> GetByTransactionIdAsync(string transactionId)
        {
            try
            {
                var payment = await _paymentTransactionRepository.GetByTransactionIdAsync(transactionId);
                if (payment != null && payment.IsActive == false)
                {
                    return null;
                }
                return payment == null ? null : MapToDto(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PaymentTransaction by TransactionID: {TransactionId}", transactionId);
                return null;
            }
        }

        public async Task<PaymentTransactionDto?> GetPaymentBySubscriptionIdAsync(Guid subscriptionId)
        {
            try
            {
                var payment = await _paymentTransactionRepository.GetPaymentBySubscriptionIdAsync(subscriptionId);
                if (payment != null && payment.IsActive == false)
                {
                    return null;
                }
                return payment == null ? null : MapToDto(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PaymentTransaction by SubscriptionID: {SubscriptionId}", subscriptionId);
                return null;
            }
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var convos = await _paymentTransactionRepository.GetByUserIdAsync(userId, page, pageSize);
                return convos.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PaymentTransactions by user ID: {UserId}", userId);
                return Enumerable.Empty<PaymentTransactionDto>();
            }
        }

        public async Task<PaymentTransactionDto> AddAsync(PaymentTransactionDto dto)
        {
            try
            {
                var payment = new PaymentTransaction
                {
                    UserId = dto.UserId,
                    SubscriptionId = dto.SubscriptionId,
                    Amount = dto.Amount,
                    PaymentMethod = dto.PaymentMethod,
                    TransactionDate = dto.TransactionDate,
                    Status = dto.Status,
                    TransactionId = dto.TransactionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdPayment = await _paymentTransactionRepository.AddAsync(payment);
                return MapToDto(createdPayment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating paymentTransaction.");
                return null;
            }
        }

        public async Task<PaymentTransactionDto> UpdateAsync(Guid id, PaymentTransactionDto dto)
        {
            try
            {
                var existing = await _paymentTransactionRepository.GetByIdAsync(id);
                if (existing == null) return null;
                existing.Status = dto.Status;
                if(dto.TransactionId != null)
                {
                    existing.TransactionId = dto.TransactionId;
                }
                existing.UpdatedAt = DateTime.UtcNow;
                await _paymentTransactionRepository.UpdateAsync(existing);
                return MapToDto(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PaymentTransaction {PaymentTransactionId}", id);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var payment = await _paymentTransactionRepository.GetByIdAsync(id);
                if (payment == null)
                {
                    _logger.LogWarning("PaymentTransaction not found for deactivation: {PaymentTransactionId}", id);
                    return false;
                }

                payment.IsActive = false;
                payment.UpdatedAt = DateTime.UtcNow;
                var result = await _paymentTransactionRepository.UpdateAsync(payment);
                if (result != null)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating payment: {PaymentTransactionId}", id);
                return false;
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _paymentTransactionRepository.GetTotalCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paymentTransaction count.");
                return 0;
            }
        }

        public async Task<int> GetTotalCountByStatusAsync(string status)
        {
            try
            {
                return await _paymentTransactionRepository.GetTotalCountByStatusAsync(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paymentTransaction by status count.");
                return 0;
            }
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _paymentTransactionRepository.GetTotalCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paymentTransaction by month count.");
                return 0;
            }
        }

        public async Task<int> GetTotalCountByStatusAndMonthAsync(string status, int year, int month)
        {
            try
            {
                return await _paymentTransactionRepository.GetTotalCountByStatusAndMonthAsync(status, year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paymentTransaction by status and month count.");
                return 0;
            }
        }

        public async Task<decimal> GetTotalAmountAsync()
        {
            try
            {
                return await _paymentTransactionRepository.GetTotalAmountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paymentTransaction amount.");
                return 0;
            }
        }

        public async Task<decimal> GetTotalAmountByMonthAsync(int year, int month)
        {
            try
            {
                return await _paymentTransactionRepository.GetTotalAmountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paymentTransaction amount by month.");
                return 0;
            }
        }

        public async Task<int> GetTotalByUserIdCountAsync(Guid userId)
        {
            try
            {
                return await _paymentTransactionRepository.GetTotalByUserIdCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paymentTransaction by user id count.");
                return 0;
            }
        }

        public static PaymentTransactionDto MapToDto(PaymentTransaction payment)
        {
            return new PaymentTransactionDto
            {
                Id = payment.Id,
                UserId = payment.UserId,
                SubscriptionId = payment.SubscriptionId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                TransactionDate = payment.TransactionDate,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}

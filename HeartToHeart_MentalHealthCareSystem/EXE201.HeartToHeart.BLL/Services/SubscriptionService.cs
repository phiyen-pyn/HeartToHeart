using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using EXE201.HeartToHeart.DAL.Repostories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository, ILogger<SubscriptionService> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetAllAsync(List<string> role, int page = 1, int pageSize = 10)
        {
            try
            {
                var subs = await _subscriptionRepository.GetAllAsync(page, pageSize);
                if (!role.Contains(Roles.Admin) && !role.Contains(Roles.Staff))
                {
                    subs = subs.Where(p => p.IsActive == true);
                }
                return subs.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all Subscriptions");
                return Enumerable.Empty<SubscriptionDto>();
            }
        }

        public async Task<SubscriptionDto?> GetByIdAsync(List<string> role, Guid id)
        {
            try
            {
                var sub = await _subscriptionRepository.GetByIdAsync(id);
                if (!role.Contains(Roles.Admin) && !role.Contains(Roles.Staff))
                {
                    if (sub != null && sub.IsActive == false)
                    {
                        return null;
                    }
                }
                return sub == null ? null : MapToDto(sub);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Subscription by ID: {SubscriptionId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<SubscriptionDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var convos = await _subscriptionRepository.GetByUserIdAsync(userId, page, pageSize);
                return convos.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Subscriptions by user ID: {UserId}", userId);
                return Enumerable.Empty<SubscriptionDto>();
            }
        }

        public async Task<SubscriptionDto?> GetLatestByUserIdAsync(Guid userId)
        {
            try
            {
                var sub = await _subscriptionRepository.GetLatestByUserIdAsync(userId);
                return sub == null ? null : MapToDto(sub);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest Subscription by user ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<SubscriptionDto> AddAsync(SubscriptionDto dto)
        {
            try
            {
                var sub = new Subscription
                {
                    UserId = dto.UserId,
                    PlanName = dto.PlanName,
                    Amount = dto.Amount,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Status = dto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdSub = await _subscriptionRepository.AddAsync(sub);
                return MapToDto(createdSub);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription.");
                return null;
            }
        }

        public async Task<SubscriptionDto> UpdateAsync(Guid id, SubscriptionDto dto)
        {
            try
            {
                var existing = await _subscriptionRepository.GetByIdAsync(id);
                if (existing == null) return null;
                existing.Status = dto.Status;
                existing.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(existing);
                return MapToDto(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Subscription {SubscriptionId}", id);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var sub = await _subscriptionRepository.GetByIdAsync(id);
                if (sub == null)
                {
                    _logger.LogWarning("Subscription not found for deactivation: {SubscriptionId}", id);
                    return false;
                }

                sub.IsActive = false;
                sub.UpdatedAt = DateTime.UtcNow;
                var result = await _subscriptionRepository.UpdateAsync(sub);
                if (result != null)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating comment: {CommentId}", id);
                return false;
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _subscriptionRepository.GetTotalCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total subscription count.");
                return 0;
            }
        }

        public async Task<int> GetTotalByUserIdCountAsync(Guid userId)
        {
            try
            {
                return await _subscriptionRepository.GetTotalByUserIdCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total subscription by user id count.");
                return 0;
            }
        }

        public async Task<IEnumerable<SubscriptionDto>> GetAllExpiredButStillActiveAsync(DateTime currentTime)
        {
            try
            {
                var subs = await _subscriptionRepository.GetExpiredSubscriptionsAsync(currentTime);
                return subs.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all expired but still active Subscription");
                return null;
            }
        }

        public async Task<IEnumerable<SubscriptionDto>> GetPendingSubscriptionsFromYesterdayAsync()
        {
            try
            {
                var subs = await _subscriptionRepository.GetPendingSubscriptionsFromYesterdayAsync();
                return subs.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending subscriptions from yesterday");
                return Enumerable.Empty<SubscriptionDto>();
            }
        }

        public static SubscriptionDto MapToDto(Subscription sub)
        {
            return new SubscriptionDto
            {
                Id = sub.Id,
                UserId = sub.UserId,
                PlanName = sub.PlanName,
                Amount = sub.Amount,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                Status = sub.Status,
                CreatedAt = sub.CreatedAt,
                UpdatedAt = sub.UpdatedAt,
                IsActive = sub.IsActive
            };
        }
    }
}

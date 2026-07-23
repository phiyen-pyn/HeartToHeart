using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<SubscriptionDto>> GetAllAsync(List<string> role, int page = 1, int pageSize = 10);
        Task<IEnumerable<SubscriptionDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<SubscriptionDto?> GetLatestByUserIdAsync(Guid userId);
        Task<SubscriptionDto?> GetByIdAsync(List<string> role, Guid id);
        Task<SubscriptionDto> AddAsync(SubscriptionDto dto);
        Task<SubscriptionDto> UpdateAsync(Guid id, SubscriptionDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetTotalCountAsync();
        Task<int> GetTotalByUserIdCountAsync(Guid userId);
        Task<IEnumerable<SubscriptionDto>> GetAllExpiredButStillActiveAsync(DateTime currentTime);
        Task<IEnumerable<SubscriptionDto>> GetPendingSubscriptionsFromYesterdayAsync();
    }
}

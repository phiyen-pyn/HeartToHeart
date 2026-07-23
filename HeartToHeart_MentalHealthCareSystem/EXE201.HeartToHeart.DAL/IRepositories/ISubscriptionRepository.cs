using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface ISubscriptionRepository
    {
        Task<IEnumerable<Subscription>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<Subscription?> GetLatestByUserIdAsync(Guid userId);
        Task<Subscription?> GetByIdAsync(Guid id);
        Task<Subscription> AddAsync(Subscription subscription);
        Task<Subscription> UpdateAsync(Subscription subscription);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetTotalCountAsync();
        Task<int> GetTotalByUserIdCountAsync(Guid userId);
        Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync(DateTime currentTime);
        Task<IEnumerable<Subscription>> GetPendingSubscriptionsFromYesterdayAsync();
    }
}

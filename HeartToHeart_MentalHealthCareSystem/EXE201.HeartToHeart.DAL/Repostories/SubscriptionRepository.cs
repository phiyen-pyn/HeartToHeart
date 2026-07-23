using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subscription>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Subscriptions
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.Subscriptions
                .Where(p => p.UserId == userId && p.Status == SubscriptionStatus.Active && p.IsActive == true)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Subscription?> GetLatestByUserIdAsync(Guid userId)
        {
            return await _context.Subscriptions
                .Where(p => p.UserId == userId && p.Status == SubscriptionStatus.Active)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Subscription?> GetByIdAsync(Guid id)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Subscription> AddAsync(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<Subscription> UpdateAsync(Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
                return false;
            _context.Subscriptions.Remove(subscription);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Subscriptions.CountAsync();
        }

        public async Task<int> GetTotalByUserIdCountAsync(Guid userId)
        {
            return await _context.Subscriptions.Where(p => p.UserId == userId && p.IsActive == true).CountAsync();
        }

        public async Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync(DateTime currentTime)
        {
            return await _context.Subscriptions
                .Where(s => s.EndDate <= currentTime && (s.Status == SubscriptionStatus.Active))
                .ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetPendingSubscriptionsFromYesterdayAsync()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            return await _context.Subscriptions
                .Where(s => s.StartDate.Date == yesterday && s.Status == SubscriptionStatus.Pending)
                .ToListAsync();
        }
    }
}

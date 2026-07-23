using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IAnonymousPostRepository
    {
        Task<AnonymousPost?> GetByIdAsync(Guid postId);
        Task<IEnumerable<AnonymousPost>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<AnonymousPost>> GetAllByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<AnonymousPost>> GetReportedPostsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<AnonymousPost>> GetReportedPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<bool> CreateAsync(AnonymousPost post);
        Task<bool> UpdateAsync(AnonymousPost post);
        Task<bool> DeleteAsync(Guid postId);
        Task<bool> ExistsAsync(Guid postId);
        Task<int> GetTotalCountAsync();
        Task<int> GetTotalByUserIdCountAsync(Guid userId);
        Task<int> GetTotalReportedCountAsync();
        Task<int> GetTotalReportedByUserIdCountAsync(Guid userId);
        Task<int> GetTotalCountByMonthAsync(int year, int month);
        Task<int> GetTotalReportedCountByMonthAsync(int year, int month);
    }
}

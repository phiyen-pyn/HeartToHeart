using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IAnonymousPostService
    {
        Task<AnonymousPostDto?> GetByIdAsync(List<string> role, Guid postId);
        Task<IEnumerable<AnonymousPostDto>> GetAllAsync(List<string> role, int page = 1, int pageSize = 10);
        Task<IEnumerable<AnonymousPostDto>> GetAllByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<AnonymousPostDto>> GetReportedPostsAsync(List<string> role, int page = 1, int pageSize = 10);
        Task<IEnumerable<AnonymousPostDto>> GetReportedPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<bool> CreateAsync(AnonymousPostDto postDto);
        Task<bool> UpdateAsync(Guid postId, AnonymousPostDto postDto);
        Task<bool> DeleteAsync(Guid postId);
        Task<int> GetTotalCountAsync();
        Task<int> GetTotalByUserIdCountAsync(Guid userId);
        Task<int> GetTotalReportedCountAsync();
        Task<int> GetTotalReportedByUserIdCountAsync(Guid userId);
        Task<int> GetTotalCountByMonthAsync(int year, int month);
        Task<int> GetTotalReportedCountByMonthAsync(int year, int month);
    }
}

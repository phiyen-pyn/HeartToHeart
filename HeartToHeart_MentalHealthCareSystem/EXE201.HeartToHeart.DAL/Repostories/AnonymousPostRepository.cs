using Azure;
using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class AnonymousPostRepository : IAnonymousPostRepository
    {
        private readonly ApplicationDbContext _context;

        public AnonymousPostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(AnonymousPost post)
        {
            try
            {
                _context.AnonymousPosts.Add(post);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid postId)
        {
            var post = await GetByIdAsync(postId);
            if (post == null) return false;

            try
            {
                _context.AnonymousPosts.Remove(post);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(Guid postId)
        {
            return await _context.AnonymousPosts.AnyAsync(p => p.Id == postId);
        }

        public async Task<IEnumerable<AnonymousPost>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.AnonymousPosts
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnonymousPost>> GetAllByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.AnonymousPosts
                .Where(p => p.UserId == userId && p.IsActive == true)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnonymousPost>> GetReportedPostsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.AnonymousPosts
                .Where(p => p.IsReported == true)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnonymousPost>> GetReportedPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.AnonymousPosts
                .Where(p => p.IsReported == true && p.UserId == userId && p.IsActive == true)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<AnonymousPost?> GetByIdAsync(Guid postId)
        {
            return await _context.AnonymousPosts
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<bool> UpdateAsync(AnonymousPost post)
        {
            try
            {
                _context.AnonymousPosts.Update(post);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.AnonymousPosts.CountAsync();
        }

        public async Task<int> GetTotalByUserIdCountAsync(Guid userId)
        {
            return await _context.AnonymousPosts.Where(p => p.UserId == userId && p.IsActive == true).CountAsync();
        }

        public async Task<int> GetTotalReportedCountAsync()
        {
            return await _context.AnonymousPosts.Where(p => p.IsReported == true).CountAsync();
        }

        public async Task<int> GetTotalReportedByUserIdCountAsync(Guid userId)
        {
            return await _context.AnonymousPosts.Where(p => p.IsReported == true && p.UserId == userId && p.IsActive == true).CountAsync();
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            return await _context.AnonymousPosts
                .Where(p => p.CreatedAt.Year == year && p.CreatedAt.Month == month)
                .CountAsync();
        }

        public async Task<int> GetTotalReportedCountByMonthAsync(int year, int month)
        {
            return await _context.AnonymousPosts
                .Where(p => p.IsReported == true && p.CreatedAt.Year == year && p.CreatedAt.Month == month)
                .CountAsync();
        }
    }
}

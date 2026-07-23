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
    public class DiaryEntryRepository : IDiaryEntryRepository
    {
        private readonly ApplicationDbContext _context;

        public DiaryEntryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DiaryEntry?> GetDiaryEntryByIdAsync(Guid entryId)
        {
            return await _context.DiaryEntries
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == entryId && d.IsActive);
        }

        public async Task<IEnumerable<DiaryEntry>> GetUserDiaryEntriesAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.DiaryEntries
                .Where(d => d.UserId == userId && d.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<DiaryEntry>> GetUserDiaryEntriesByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _context.DiaryEntries
                .Where(d => d.UserId == userId && d.IsActive &&
                           d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<DiaryEntry?> CreateDiaryEntryAsync(DiaryEntry diaryEntry)
        {
            try
            {
                _context.DiaryEntries.Add(diaryEntry);
                await _context.SaveChangesAsync();
                return diaryEntry;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateDiaryEntryAsync(DiaryEntry diaryEntry)
        {
            try
            {
                _context.DiaryEntries.Update(diaryEntry);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDiaryEntryAsync(Guid entryId)
        {
            try
            {
                var entry = await GetDiaryEntryByIdAsync(entryId);
                if (entry == null) return false;

                entry.IsActive = false;
                entry.UpdatedAt = DateTime.UtcNow;

                _context.DiaryEntries.Update(entry);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DiaryEntryExistsAsync(Guid entryId)
        {
            return await _context.DiaryEntries
                .AnyAsync(d => d.Id == entryId && d.IsActive);
        }

        public async Task<int> GetUserDiaryEntriesCountAsync(Guid userId)
        {
            return await _context.DiaryEntries
                .CountAsync(d => d.UserId == userId && d.IsActive);
        }

        public async Task<IEnumerable<DiaryEntry>> SearchUserDiaryEntriesAsync(Guid userId, string searchTerm)
        {
            return await _context.DiaryEntries
                .Where(d => d.UserId == userId && d.IsActive &&
                           (d.Title.Contains(searchTerm) || d.Content.Contains(searchTerm)))
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<DiaryEntry?> GetLatestDiaryEntryAsync(Guid userId)
        {
            return await _context.DiaryEntries
                .Where(d => d.UserId == userId && d.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}

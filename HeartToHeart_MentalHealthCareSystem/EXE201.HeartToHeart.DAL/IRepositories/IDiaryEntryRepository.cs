using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IDiaryEntryRepository
    {
        Task<DiaryEntry?> GetDiaryEntryByIdAsync(Guid entryId);
        Task<IEnumerable<DiaryEntry>> GetUserDiaryEntriesAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<DiaryEntry>> GetUserDiaryEntriesByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<DiaryEntry?> CreateDiaryEntryAsync(DiaryEntry diaryEntry);
        Task<bool> UpdateDiaryEntryAsync(DiaryEntry diaryEntry);
        Task<bool> DeleteDiaryEntryAsync(Guid entryId);
        Task<bool> DiaryEntryExistsAsync(Guid entryId);
        Task<int> GetUserDiaryEntriesCountAsync(Guid userId);
        Task<IEnumerable<DiaryEntry>> SearchUserDiaryEntriesAsync(Guid userId, string searchTerm);
        Task<DiaryEntry?> GetLatestDiaryEntryAsync(Guid userId);
    }
}

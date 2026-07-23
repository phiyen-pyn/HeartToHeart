using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IDiaryEntryService
    {
        Task<DiaryEntryDto?> GetDiaryEntryByIdAsync(Guid entryId, Guid userId);
        Task<IEnumerable<DiaryEntryDto>> GetUserDiaryEntriesAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<DiaryEntryDto>> GetUserDiaryEntriesByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<DiaryEntryDto?> CreateDiaryEntryAsync(Guid userId, CreateDiaryEntryDto createDto, string? createdBy = null);
        Task<bool> UpdateDiaryEntryAsync(Guid entryId, Guid userId, UpdateDiaryEntryDto updateDto, string? updatedBy = null);
        Task<bool> DeleteDiaryEntryAsync(Guid entryId, Guid userId);
        Task<int> GetUserDiaryEntriesCountAsync(Guid userId);
        Task<IEnumerable<DiaryEntryDto>> SearchUserDiaryEntriesAsync(Guid userId, string searchTerm);
        Task<DiaryEntryDto?> GetLatestDiaryEntryAsync(Guid userId);
    }
}

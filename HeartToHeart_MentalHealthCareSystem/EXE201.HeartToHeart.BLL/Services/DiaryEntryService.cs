using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class DiaryEntryService : IDiaryEntryService
    {
        private readonly IDiaryEntryRepository _diaryEntryRepository;
        private readonly ILogger<DiaryEntryService> _logger;

        public DiaryEntryService(
            IDiaryEntryRepository diaryEntryRepository,
            ILogger<DiaryEntryService> logger)
        {
            _diaryEntryRepository = diaryEntryRepository;
            _logger = logger;
        }

        public async Task<DiaryEntryDto?> GetDiaryEntryByIdAsync(Guid entryId, Guid userId)
        {
            try
            {
                var entry = await _diaryEntryRepository.GetDiaryEntryByIdAsync(entryId);

                if (entry == null || entry.UserId != userId)
                {
                    _logger.LogWarning("Diary entry not found or access denied. EntryId: {EntryId}, UserId: {UserId}", entryId, userId);
                    return null;
                }

                return MapToDiaryEntryDto(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting diary entry by ID: {EntryId}", entryId);
                return null;
            }
        }

        public async Task<IEnumerable<DiaryEntryDto>> GetUserDiaryEntriesAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var entries = await _diaryEntryRepository.GetUserDiaryEntriesAsync(userId, page, pageSize);
                return entries.Select(MapToDiaryEntryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user diary entries. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);
                return Enumerable.Empty<DiaryEntryDto>();
            }
        }

        public async Task<IEnumerable<DiaryEntryDto>> GetUserDiaryEntriesByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var entries = await _diaryEntryRepository.GetUserDiaryEntriesByDateRangeAsync(userId, startDate, endDate);
                return entries.Select(MapToDiaryEntryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user diary entries by date range. UserId: {UserId}, StartDate: {StartDate}, EndDate: {EndDate}", userId, startDate, endDate);
                return Enumerable.Empty<DiaryEntryDto>();
            }
        }

        public async Task<DiaryEntryDto?> CreateDiaryEntryAsync(Guid userId, CreateDiaryEntryDto createDto, string? createdBy = null)
        {
            try
            {
                var diaryEntry = new DiaryEntry
                {
                    UserId = userId,
                    Title = createDto.Title,
                    Content = createDto.Content,
                    CreatedBy = createdBy,
                    UpdatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdEntry = await _diaryEntryRepository.CreateDiaryEntryAsync(diaryEntry);

                if (createdEntry == null)
                {
                    _logger.LogWarning("Failed to create diary entry for user: {UserId}", userId);
                    return null;
                }

                _logger.LogInformation("Diary entry created successfully. EntryId: {EntryId}, UserId: {UserId}", createdEntry.Id, userId);
                return MapToDiaryEntryDto(createdEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating diary entry for user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateDiaryEntryAsync(Guid entryId, Guid userId, UpdateDiaryEntryDto updateDto, string? updatedBy = null)
        {
            try
            {
                var entry = await _diaryEntryRepository.GetDiaryEntryByIdAsync(entryId);

                if (entry == null || entry.UserId != userId)
                {
                    _logger.LogWarning("Diary entry not found or access denied for update. EntryId: {EntryId}, UserId: {UserId}", entryId, userId);
                    return false;
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(updateDto.Title))
                    entry.Title = updateDto.Title;

                if (!string.IsNullOrEmpty(updateDto.Content))
                    entry.Content = updateDto.Content;

                entry.UpdatedBy = updatedBy;
                entry.UpdatedAt = DateTime.UtcNow;

                var result = await _diaryEntryRepository.UpdateDiaryEntryAsync(entry);

                if (result)
                    _logger.LogInformation("Diary entry updated successfully. EntryId: {EntryId}, UserId: {UserId}", entryId, userId);
                else
                    _logger.LogWarning("Failed to update diary entry. EntryId: {EntryId}, UserId: {UserId}", entryId, userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating diary entry: {EntryId}", entryId);
                return false;
            }
        }

        public async Task<bool> DeleteDiaryEntryAsync(Guid entryId, Guid userId)
        {
            try
            {
                var entry = await _diaryEntryRepository.GetDiaryEntryByIdAsync(entryId);

                if (entry == null || entry.UserId != userId)
                {
                    _logger.LogWarning("Diary entry not found or access denied for deletion. EntryId: {EntryId}, UserId: {UserId}", entryId, userId);
                    return false;
                }

                var result = await _diaryEntryRepository.DeleteDiaryEntryAsync(entryId);

                if (result)
                    _logger.LogInformation("Diary entry deleted successfully. EntryId: {EntryId}, UserId: {UserId}", entryId, userId);
                else
                    _logger.LogWarning("Failed to delete diary entry. EntryId: {EntryId}, UserId: {UserId}", entryId, userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting diary entry: {EntryId}", entryId);
                return false;
            }
        }

        public async Task<int> GetUserDiaryEntriesCountAsync(Guid userId)
        {
            try
            {
                return await _diaryEntryRepository.GetUserDiaryEntriesCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user diary entries count: {UserId}", userId);
                return 0;
            }
        }

        public async Task<IEnumerable<DiaryEntryDto>> SearchUserDiaryEntriesAsync(Guid userId, string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return Enumerable.Empty<DiaryEntryDto>();

                var entries = await _diaryEntryRepository.SearchUserDiaryEntriesAsync(userId, searchTerm);
                return entries.Select(MapToDiaryEntryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching user diary entries. UserId: {UserId}, SearchTerm: {SearchTerm}", userId, searchTerm);
                return Enumerable.Empty<DiaryEntryDto>();
            }
        }

        public async Task<DiaryEntryDto?> GetLatestDiaryEntryAsync(Guid userId)
        {
            try
            {
                var entry = await _diaryEntryRepository.GetLatestDiaryEntryAsync(userId);
                return entry == null ? null : MapToDiaryEntryDto(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest diary entry for user: {UserId}", userId);
                return null;
            }
        }

        private static DiaryEntryDto MapToDiaryEntryDto(DiaryEntry entry)
        {
            return new DiaryEntryDto
            {
                Id = entry.Id,
                UserId = entry.UserId,
                Title = entry.Title ?? string.Empty,
                Content = entry.Content ?? string.Empty,
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt,
                IsActive = entry.IsActive,
                CreatedBy = entry.CreatedBy,
                UpdatedBy = entry.UpdatedBy
            };
        }
    }
}

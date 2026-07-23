using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IEmotionTrackService
    {
        Task<EmotionTrackDto?> GetEmotionTrackByIdAsync(Guid trackId, Guid userId);
        Task<IEnumerable<EmotionTrackDto>> GetUserEmotionTracksAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<EmotionTrackDto>> GetUserEmotionTracksByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<DailyEmotionSummaryDto?> GetUserEmotionTracksByDateAsync(Guid userId, DateTime date);
        Task<IEnumerable<EmotionTrackDto>> GetUserEmotionTracksByEmotionAsync(Guid userId, string emotion);
        Task<EmotionTrackDto?> CreateEmotionTrackAsync(Guid userId, CreateEmotionTrackDto createDto, string? createdBy = null);
        Task<bool> UpdateEmotionTrackAsync(Guid trackId, Guid userId, UpdateEmotionTrackDto updateDto, string? updatedBy = null);
        Task<bool> DeleteEmotionTrackAsync(Guid trackId, Guid userId);
        Task<int> GetUserEmotionTracksCountAsync(Guid userId);
        Task<EmotionTrackDto?> GetLatestEmotionTrackAsync(Guid userId);
        Task<IEnumerable<EmotionTrackDto>> GetTodayEmotionTracksAsync(Guid userId);
        Task<EmotionStatsDto> GetEmotionStatsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<DailyEmotionSummaryDto>> GetEmotionTrendsAsync(Guid userId, int days = 30);
        Task<List<string>> GetAvailableEmotionsAsync();
    }
}

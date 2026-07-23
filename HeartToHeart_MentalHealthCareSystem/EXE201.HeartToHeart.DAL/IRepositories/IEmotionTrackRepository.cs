using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IEmotionTrackRepository
    {
        Task<EmotionTrack?> GetEmotionTrackByIdAsync(Guid trackId);
        Task<IEnumerable<EmotionTrack>> GetUserEmotionTracksAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<EmotionTrack>> GetUserEmotionTracksByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<EmotionTrack>> GetUserEmotionTracksByDateAsync(Guid userId, DateTime date);
        Task<IEnumerable<EmotionTrack>> GetUserEmotionTracksByEmotionAsync(Guid userId, string emotion);
        Task<EmotionTrack?> CreateEmotionTrackAsync(EmotionTrack emotionTrack);
        Task<bool> UpdateEmotionTrackAsync(EmotionTrack emotionTrack);
        Task<bool> DeleteEmotionTrackAsync(Guid trackId);
        Task<bool> EmotionTrackExistsAsync(Guid trackId);
        Task<int> GetUserEmotionTracksCountAsync(Guid userId);
        Task<EmotionTrack?> GetLatestEmotionTrackAsync(Guid userId);
        Task<IEnumerable<EmotionTrack>> GetTodayEmotionTracksAsync(Guid userId);
        Task<Dictionary<string, int>> GetEmotionFrequencyAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<double> GetAverageIntensityAsync(Guid userId, string? emotion = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<EmotionTrack>> GetEmotionTrendsAsync(Guid userId, int days = 30);
    }
}

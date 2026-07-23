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
    public class EmotionTrackRepository : IEmotionTrackRepository
    {
        private readonly ApplicationDbContext _context;

        public EmotionTrackRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmotionTrack?> GetEmotionTrackByIdAsync(Guid trackId)
        {
            return await _context.EmotionTracks
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == trackId && e.IsActive);
        }

        public async Task<IEnumerable<EmotionTrack>> GetUserEmotionTracksAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmotionTrack>> GetUserEmotionTracksByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive &&
                           e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmotionTrack>> GetUserEmotionTracksByDateAsync(Guid userId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            return await _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive &&
                           e.CreatedAt >= startOfDay && e.CreatedAt <= endOfDay)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmotionTrack>> GetUserEmotionTracksByEmotionAsync(Guid userId, string emotion)
        {
            return await _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive &&
                           e.Emotion == emotion)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<EmotionTrack?> CreateEmotionTrackAsync(EmotionTrack emotionTrack)
        {
            try
            {
                _context.EmotionTracks.Add(emotionTrack);
                await _context.SaveChangesAsync();
                return emotionTrack;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateEmotionTrackAsync(EmotionTrack emotionTrack)
        {
            try
            {
                _context.EmotionTracks.Update(emotionTrack);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteEmotionTrackAsync(Guid trackId)
        {
            try
            {
                var track = await GetEmotionTrackByIdAsync(trackId);
                if (track == null) return false;

                track.IsActive = false;
                track.UpdatedAt = DateTime.UtcNow;

                _context.EmotionTracks.Update(track);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EmotionTrackExistsAsync(Guid trackId)
        {
            return await _context.EmotionTracks
                .AnyAsync(e => e.Id == trackId && e.IsActive);
        }

        public async Task<int> GetUserEmotionTracksCountAsync(Guid userId)
        {
            return await _context.EmotionTracks
                .CountAsync(e => e.UserId == userId && e.IsActive);
        }

        public async Task<EmotionTrack?> GetLatestEmotionTrackAsync(Guid userId)
        {
            return await _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EmotionTrack>> GetTodayEmotionTracksAsync(Guid userId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive &&
                           e.CreatedAt >= today && e.CreatedAt < tomorrow)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetEmotionFrequencyAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive);

            if (startDate.HasValue)
                query = query.Where(e => e.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.CreatedAt <= endDate.Value);

            return await query
                .GroupBy(e => e.Emotion)
                .ToDictionaryAsync(g => g.Key ?? "Unknown", g => g.Count());
        }

        public async Task<double> GetAverageIntensityAsync(Guid userId, string? emotion = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive && e.IntensityLevel.HasValue);

            if (!string.IsNullOrEmpty(emotion))
                query = query.Where(e => e.Emotion == emotion);

            if (startDate.HasValue)
                query = query.Where(e => e.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.CreatedAt <= endDate.Value);

            var intensities = await query.Select(e => e.IntensityLevel!.Value).ToListAsync();
            return intensities.Any() ? intensities.Average() : 0;
        }

        public async Task<IEnumerable<EmotionTrack>> GetEmotionTrendsAsync(Guid userId, int days = 30)
        {
            var startDate = DateTime.Today.AddDays(-days);

            return await _context.EmotionTracks
                .Where(e => e.UserId == userId && e.IsActive &&
                           e.CreatedAt >= startDate)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
    }
}

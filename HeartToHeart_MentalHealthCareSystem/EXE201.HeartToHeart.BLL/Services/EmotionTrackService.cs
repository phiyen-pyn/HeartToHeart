using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
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
    public class EmotionTrackService : IEmotionTrackService
    {
        private readonly IEmotionTrackRepository _emotionTrackRepository;
        private readonly ILogger<EmotionTrackService> _logger;

        public EmotionTrackService(
            IEmotionTrackRepository emotionTrackRepository,
            ILogger<EmotionTrackService> logger)
        {
            _emotionTrackRepository = emotionTrackRepository;
            _logger = logger;
        }

        public async Task<EmotionTrackDto?> GetEmotionTrackByIdAsync(Guid trackId, Guid userId)
        {
            try
            {
                var track = await _emotionTrackRepository.GetEmotionTrackByIdAsync(trackId);

                if (track == null || track.UserId != userId)
                {
                    _logger.LogWarning("Emotion track not found or access denied. TrackId: {TrackId}, UserId: {UserId}", trackId, userId);
                    return null;
                }

                return MapToEmotionTrackDto(track);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emotion track by ID: {TrackId}", trackId);
                return null;
            }
        }

        public async Task<IEnumerable<EmotionTrackDto>> GetUserEmotionTracksAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var tracks = await _emotionTrackRepository.GetUserEmotionTracksAsync(userId, page, pageSize);
                return tracks.Select(MapToEmotionTrackDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user emotion tracks. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);
                return Enumerable.Empty<EmotionTrackDto>();
            }
        }

        public async Task<IEnumerable<EmotionTrackDto>> GetUserEmotionTracksByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var tracks = await _emotionTrackRepository.GetUserEmotionTracksByDateRangeAsync(userId, startDate, endDate);
                return tracks.Select(MapToEmotionTrackDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user emotion tracks by date range. UserId: {UserId}, StartDate: {StartDate}, EndDate: {EndDate}", userId, startDate, endDate);
                return Enumerable.Empty<EmotionTrackDto>();
            }
        }

        public async Task<DailyEmotionSummaryDto?> GetUserEmotionTracksByDateAsync(Guid userId, DateTime date)
        {
            try
            {
                var tracks = await _emotionTrackRepository.GetUserEmotionTracksByDateAsync(userId, date);
                var emotionDtos = tracks.Select(MapToEmotionTrackDto).ToList();

                if (!emotionDtos.Any())
                    return null;

                var dominantEmotion = emotionDtos
                    .GroupBy(e => e.Emotion)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key;

                var averageIntensity = emotionDtos
                    .Where(e => e.IntensityLevel.HasValue)
                    .Select(e => e.IntensityLevel!.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                return new DailyEmotionSummaryDto
                {
                    Date = date.Date,
                    Emotions = emotionDtos,
                    DominantEmotion = dominantEmotion,
                    AverageIntensity = averageIntensity,
                    TotalRecords = emotionDtos.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user emotion tracks by date. UserId: {UserId}, Date: {Date}", userId, date);
                return null;
            }
        }

        public async Task<IEnumerable<EmotionTrackDto>> GetUserEmotionTracksByEmotionAsync(Guid userId, string emotion)
        {
            try
            {
                var tracks = await _emotionTrackRepository.GetUserEmotionTracksByEmotionAsync(userId, emotion);
                return tracks.Select(MapToEmotionTrackDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user emotion tracks by emotion. UserId: {UserId}, Emotion: {Emotion}", userId, emotion);
                return Enumerable.Empty<EmotionTrackDto>();
            }
        }

        public async Task<EmotionTrackDto?> CreateEmotionTrackAsync(Guid userId, CreateEmotionTrackDto createDto, string? createdBy = null)
        {
            try
            {
                // Normalize emotion using the constants
                var normalizedEmotion = Emotions.GetNormalizedEmotion(createDto.Emotion);

                var emotionTrack = new EmotionTrack
                {
                    UserId = userId,
                    Emotion = normalizedEmotion,
                    Note = createDto.Note,
                    IntensityLevel = createDto.IntensityLevel,
                    CreatedBy = createdBy,
                    UpdatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdTrack = await _emotionTrackRepository.CreateEmotionTrackAsync(emotionTrack);

                if (createdTrack == null)
                {
                    _logger.LogWarning("Failed to create emotion track for user: {UserId}", userId);
                    return null;
                }

                _logger.LogInformation("Emotion track created successfully. TrackId: {TrackId}, UserId: {UserId}, Emotion: {Emotion}", createdTrack.Id, userId, normalizedEmotion);
                return MapToEmotionTrackDto(createdTrack);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emotion track for user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateEmotionTrackAsync(Guid trackId, Guid userId, UpdateEmotionTrackDto updateDto, string? updatedBy = null)
        {
            try
            {
                var track = await _emotionTrackRepository.GetEmotionTrackByIdAsync(trackId);

                if (track == null || track.UserId != userId)
                {
                    _logger.LogWarning("Emotion track not found or access denied for update. TrackId: {TrackId}, UserId: {UserId}", trackId, userId);
                    return false;
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(updateDto.Emotion))
                    track.Emotion = Emotions.GetNormalizedEmotion(updateDto.Emotion);

                if (updateDto.Note != null)
                    track.Note = updateDto.Note;

                if (updateDto.IntensityLevel.HasValue)
                    track.IntensityLevel = updateDto.IntensityLevel;

                track.UpdatedBy = updatedBy;
                track.UpdatedAt = DateTime.UtcNow;

                var result = await _emotionTrackRepository.UpdateEmotionTrackAsync(track);

                if (result)
                    _logger.LogInformation("Emotion track updated successfully. TrackId: {TrackId}, UserId: {UserId}", trackId, userId);
                else
                    _logger.LogWarning("Failed to update emotion track. TrackId: {TrackId}, UserId: {UserId}", trackId, userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emotion track: {TrackId}", trackId);
                return false;
            }
        }

        public async Task<bool> DeleteEmotionTrackAsync(Guid trackId, Guid userId)
        {
            try
            {
                var track = await _emotionTrackRepository.GetEmotionTrackByIdAsync(trackId);

                if (track == null || track.UserId != userId)
                {
                    _logger.LogWarning("Emotion track not found or access denied for deletion. TrackId: {TrackId}, UserId: {UserId}", trackId, userId);
                    return false;
                }

                var result = await _emotionTrackRepository.DeleteEmotionTrackAsync(trackId);

                if (result)
                    _logger.LogInformation("Emotion track deleted successfully. TrackId: {TrackId}, UserId: {UserId}", trackId, userId);
                else
                    _logger.LogWarning("Failed to delete emotion track. TrackId: {TrackId}, UserId: {UserId}", trackId, userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting emotion track: {TrackId}", trackId);
                return false;
            }
        }

        public async Task<int> GetUserEmotionTracksCountAsync(Guid userId)
        {
            try
            {
                return await _emotionTrackRepository.GetUserEmotionTracksCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user emotion tracks count: {UserId}", userId);
                return 0;
            }
        }

        public async Task<EmotionTrackDto?> GetLatestEmotionTrackAsync(Guid userId)
        {
            try
            {
                var track = await _emotionTrackRepository.GetLatestEmotionTrackAsync(userId);
                return track == null ? null : MapToEmotionTrackDto(track);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest emotion track for user: {UserId}", userId);
                return null;
            }
        }

        public async Task<IEnumerable<EmotionTrackDto>> GetTodayEmotionTracksAsync(Guid userId)
        {
            try
            {
                var tracks = await _emotionTrackRepository.GetTodayEmotionTracksAsync(userId);
                return tracks.Select(MapToEmotionTrackDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's emotion tracks for user: {UserId}", userId);
                return Enumerable.Empty<EmotionTrackDto>();
            }
        }

        public async Task<EmotionStatsDto> GetEmotionStatsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var totalCount = await _emotionTrackRepository.GetUserEmotionTracksCountAsync(userId);
                var emotionFrequency = await _emotionTrackRepository.GetEmotionFrequencyAsync(userId, startDate, endDate);
                var overallAverageIntensity = await _emotionTrackRepository.GetAverageIntensityAsync(userId, null, startDate, endDate);
                var latestTrack = await _emotionTrackRepository.GetLatestEmotionTrackAsync(userId);

                var emotionBreakdown = new List<EmotionSummaryDto>();

                foreach (var emotion in emotionFrequency)
                {
                    var avgIntensity = await _emotionTrackRepository.GetAverageIntensityAsync(userId, emotion.Key, startDate, endDate);
                    var emotionTracks = await _emotionTrackRepository.GetUserEmotionTracksByEmotionAsync(userId, emotion.Key);
                    var intensities = emotionTracks.Where(t => t.IntensityLevel.HasValue).Select(t => t.IntensityLevel!.Value).ToList();

                    emotionBreakdown.Add(new EmotionSummaryDto
                    {
                        Emotion = emotion.Key,
                        Count = emotion.Value,
                        AverageIntensity = avgIntensity,
                        MaxIntensity = intensities.Any() ? intensities.Max() : null,
                        MinIntensity = intensities.Any() ? intensities.Min() : null,
                        LastRecorded = emotionTracks.OrderByDescending(t => t.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue
                    });
                }

                // Generate daily summary for the date range
                var dailySummary = new List<DailyEmotionSummaryDto>();
                if (startDate.HasValue && endDate.HasValue)
                {
                    for (var date = startDate.Value.Date; date <= endDate.Value.Date; date = date.AddDays(1))
                    {
                        var dailyData = await GetUserEmotionTracksByDateAsync(userId, date);
                        if (dailyData != null)
                        {
                            dailySummary.Add(dailyData);
                        }
                    }
                }

                return new EmotionStatsDto
                {
                    TotalRecords = totalCount,
                    EmotionBreakdown = emotionBreakdown.OrderByDescending(e => e.Count).ToList(),
                    OverallAverageIntensity = overallAverageIntensity,
                    MostFrequentEmotion = emotionFrequency.OrderByDescending(e => e.Value).FirstOrDefault().Key,
                    LastRecorded = latestTrack?.CreatedAt,
                    DailySummary = dailySummary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emotion stats for user: {UserId}", userId);
                return new EmotionStatsDto();
            }
        }

        public async Task<IEnumerable<DailyEmotionSummaryDto>> GetEmotionTrendsAsync(Guid userId, int days = 30)
        {
            try
            {
                var trends = new List<DailyEmotionSummaryDto>();
                var startDate = DateTime.Today.AddDays(-days);

                for (var date = startDate; date <= DateTime.Today; date = date.AddDays(1))
                {
                    var dailySummary = await GetUserEmotionTracksByDateAsync(userId, date);
                    if (dailySummary != null)
                    {
                        trends.Add(dailySummary);
                    }
                    else
                    {
                        trends.Add(new DailyEmotionSummaryDto
                        {
                            Date = date.Date,
                            Emotions = new List<EmotionTrackDto>(),
                            TotalRecords = 0
                        });
                    }
                }

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emotion trends for user: {UserId}", userId);
                return Enumerable.Empty<DailyEmotionSummaryDto>();
            }
        }

        public async Task<List<string>> GetAvailableEmotionsAsync()
        {
            try
            {
                return await Task.FromResult(Emotions.AvailableEmotions.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available emotions");
                return new List<string>();
            }
        }

        private static EmotionTrackDto MapToEmotionTrackDto(EmotionTrack emotionTrack)
        {
            return new EmotionTrackDto
            {
                Id = emotionTrack.Id,
                UserId = emotionTrack.UserId,
                Emotion = emotionTrack.Emotion ?? string.Empty,
                Note = emotionTrack.Note,
                IntensityLevel = emotionTrack.IntensityLevel,
                CreatedAt = emotionTrack.CreatedAt,
                UpdatedAt = emotionTrack.UpdatedAt,
                IsActive = emotionTrack.IsActive,
                CreatedBy = emotionTrack.CreatedBy,
                UpdatedBy = emotionTrack.UpdatedBy
            };
        }
    }
}

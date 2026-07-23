using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmotionTrackController : ControllerBase
    {
        private readonly IEmotionTrackService _emotionTrackService;

        public EmotionTrackController(IEmotionTrackService emotionTrackService)
        {
            _emotionTrackService = emotionTrackService;
        }

        [HttpGet("emotions")]
        public async Task<IActionResult> GetAvailableEmotions()
        {
            var emotions = await _emotionTrackService.GetAvailableEmotionsAsync();
            return Ok(new { Emotions = emotions });
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmotionTrack([FromBody] CreateEmotionTrackDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var createdBy = GetCurrentUserName();

            var result = await _emotionTrackService.CreateEmotionTrackAsync(userId, createDto, createdBy);

            if (result == null)
                return BadRequest(new { Message = "Failed to create emotion track" });

            return CreatedAtAction(nameof(GetEmotionTrackById), new { trackId = result.Id }, result);
        }

        [HttpGet("{trackId}")]
        public async Task<IActionResult> GetEmotionTrackById(Guid trackId)
        {
            var userId = GetCurrentUserId();
            var track = await _emotionTrackService.GetEmotionTrackByIdAsync(trackId, userId);

            if (track == null)
                return NotFound(new { Message = "Emotion track not found" });

            return Ok(track);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserEmotionTracks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var tracks = await _emotionTrackService.GetUserEmotionTracksAsync(userId, page, pageSize);
            var totalCount = await _emotionTrackService.GetUserEmotionTracksCountAsync(userId);

            return Ok(new
            {
                EmotionTracks = tracks,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodayEmotionTracks()
        {
            var userId = GetCurrentUserId();
            var tracks = await _emotionTrackService.GetTodayEmotionTracksAsync(userId);

            return Ok(new { TodayEmotions = tracks });
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestEmotionTrack()
        {
            var userId = GetCurrentUserId();
            var track = await _emotionTrackService.GetLatestEmotionTrackAsync(userId);

            if (track == null)
                return NotFound(new { Message = "No emotion tracks found" });

            return Ok(track);
        }

        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetEmotionTracksByDate(DateTime date)
        {
            var userId = GetCurrentUserId();
            var summary = await _emotionTrackService.GetUserEmotionTracksByDateAsync(userId, date);

            if (summary == null)
                return NotFound(new { Message = "No emotion tracks found for the specified date" });

            return Ok(summary);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetEmotionTracksByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest(new { Message = "Start date cannot be greater than end date" });

            var userId = GetCurrentUserId();
            var tracks = await _emotionTrackService.GetUserEmotionTracksByDateRangeAsync(userId, startDate, endDate);

            return Ok(new { EmotionTracks = tracks });
        }

        [HttpGet("emotion/{emotion}")]
        public async Task<IActionResult> GetEmotionTracksByEmotion(string emotion)
        {
            if (!Emotions.IsValidEmotion(emotion))
                return BadRequest(new { Message = "Invalid emotion type" });

            var userId = GetCurrentUserId();
            var tracks = await _emotionTrackService.GetUserEmotionTracksByEmotionAsync(userId, emotion);

            return Ok(new { EmotionTracks = tracks });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetEmotionStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                return BadRequest(new { Message = "Start date cannot be greater than end date" });

            var userId = GetCurrentUserId();
            var stats = await _emotionTrackService.GetEmotionStatsAsync(userId, startDate, endDate);

            return Ok(stats);
        }

        [HttpGet("trends")]
        public async Task<IActionResult> GetEmotionTrends([FromQuery] int days = 30)
        {
            if (days <= 0 || days > 365)
                return BadRequest(new { Message = "Days must be between 1 and 365" });

            var userId = GetCurrentUserId();
            var trends = await _emotionTrackService.GetEmotionTrendsAsync(userId, days);

            return Ok(new { Trends = trends });
        }

        [HttpPut("{trackId}")]
        public async Task<IActionResult> UpdateEmotionTrack(Guid trackId, [FromBody] UpdateEmotionTrackDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var updatedBy = GetCurrentUserName();

            var result = await _emotionTrackService.UpdateEmotionTrackAsync(trackId, userId, updateDto, updatedBy);

            if (!result)
                return BadRequest(new { Message = "Failed to update emotion track" });

            return Ok(new { Message = "Emotion track updated successfully" });
        }

        [HttpDelete("{trackId}")]
        public async Task<IActionResult> DeleteEmotionTrack(Guid trackId)
        {
            var userId = GetCurrentUserId();
            var result = await _emotionTrackService.DeleteEmotionTrackAsync(trackId, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to delete emotion track" });

            return Ok(new { Message = "Emotion track deleted successfully" });
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetEmotionTracksCount()
        {
            var userId = GetCurrentUserId();
            var count = await _emotionTrackService.GetUserEmotionTracksCountAsync(userId);

            return Ok(new { TotalCount = count });
        }

        // Admin endpoints
        [HttpGet("admin/user/{userId}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Staff + "," + Roles.Counselor)]
        public async Task<IActionResult> GetUserEmotionTracksForAdmin(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var tracks = await _emotionTrackService.GetUserEmotionTracksAsync(userId, page, pageSize);
            var totalCount = await _emotionTrackService.GetUserEmotionTracksCountAsync(userId);

            return Ok(new
            {
                UserId = userId,
                EmotionTracks = tracks,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("admin/user/{userId}/stats")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Staff + "," + Roles.Counselor)]
        public async Task<IActionResult> GetUserEmotionStatsForAdmin(Guid userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                return BadRequest(new { Message = "Start date cannot be greater than end date" });

            var stats = await _emotionTrackService.GetEmotionStatsAsync(userId, startDate, endDate);

            return Ok(new { UserId = userId, Stats = stats });
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdString ?? throw new UnauthorizedAccessException("User ID not found in token"));
        }

        private string? GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}

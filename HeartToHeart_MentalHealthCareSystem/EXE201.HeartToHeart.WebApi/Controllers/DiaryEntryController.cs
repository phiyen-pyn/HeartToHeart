using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DiaryEntryController : ControllerBase
    {
        private readonly IDiaryEntryService _diaryEntryService;

        public DiaryEntryController(IDiaryEntryService diaryEntryService)
        {
            _diaryEntryService = diaryEntryService;
        }

        [HttpGet("{entryId}")]
        public async Task<IActionResult> GetDiaryEntryById(Guid entryId)
        {
            var userId = GetCurrentUserId();
            var entry = await _diaryEntryService.GetDiaryEntryByIdAsync(entryId, userId);

            if (entry == null)
                return NotFound(new { Message = "Diary entry not found" });

            return Ok(entry);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDiaryEntries([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var entries = await _diaryEntryService.GetUserDiaryEntriesAsync(userId, page, pageSize);
            var totalCount = await _diaryEntryService.GetUserDiaryEntriesCountAsync(userId);

            return Ok(new
            {
                Entries = entries,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetDiaryEntriesByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var userId = GetCurrentUserId();

            if (startDate > endDate)
                return BadRequest(new { Message = "Start date cannot be greater than end date" });

            var entries = await _diaryEntryService.GetUserDiaryEntriesByDateRangeAsync(userId, startDate, endDate);

            return Ok(new
            {
                Entries = entries,
                DateRange = new { StartDate = startDate, EndDate = endDate },
                Count = entries.Count()
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDiaryEntries([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { Message = "Search term is required" });

            var userId = GetCurrentUserId();
            var entries = await _diaryEntryService.SearchUserDiaryEntriesAsync(userId, searchTerm);

            return Ok(new
            {
                Entries = entries,
                SearchTerm = searchTerm,
                Count = entries.Count()
            });
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestDiaryEntry()
        {
            var userId = GetCurrentUserId();
            var entry = await _diaryEntryService.GetLatestDiaryEntryAsync(userId);

            if (entry == null)
                return NotFound(new { Message = "No diary entries found" });

            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiaryEntry([FromBody] CreateDiaryEntryDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            var createdEntry = await _diaryEntryService.CreateDiaryEntryAsync(userId, createDto, userName);

            if (createdEntry == null)
                return BadRequest(new { Message = "Failed to create diary entry" });

            return CreatedAtAction(nameof(GetDiaryEntryById),
                new { entryId = createdEntry.Id },
                new { Message = "Diary entry created successfully", Entry = createdEntry });
        }

        [HttpPut("{entryId}")]
        public async Task<IActionResult> UpdateDiaryEntry(Guid entryId, [FromBody] UpdateDiaryEntryDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            var result = await _diaryEntryService.UpdateDiaryEntryAsync(entryId, userId, updateDto, userName);

            if (!result)
                return BadRequest(new { Message = "Failed to update diary entry" });

            return Ok(new { Message = "Diary entry updated successfully" });
        }

        [HttpDelete("{entryId}")]
        public async Task<IActionResult> DeleteDiaryEntry(Guid entryId)
        {
            var userId = GetCurrentUserId();
            var result = await _diaryEntryService.DeleteDiaryEntryAsync(entryId, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to delete diary entry" });

            return Ok(new { Message = "Diary entry deleted successfully" });
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetDiaryEntriesCount()
        {
            var userId = GetCurrentUserId();
            var count = await _diaryEntryService.GetUserDiaryEntriesCountAsync(userId);

            return Ok(new { TotalCount = count });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        private string? GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ??
                   User.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}

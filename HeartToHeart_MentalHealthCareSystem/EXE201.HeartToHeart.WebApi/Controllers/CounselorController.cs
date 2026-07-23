using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CounselorController : ControllerBase
    {
        private readonly ICounselorService _counselorService;
        private readonly ILogger<CounselorController> _logger;

        public CounselorController(ICounselorService counselorService, ILogger<CounselorController> logger)
        {
            _counselorService = counselorService;
            _logger = logger;
        }

        /// <summary>
        /// Verify a counselor - Admin only
        /// </summary>
        [HttpPost("{counselorId}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyCounselor(Guid counselorId, [FromBody] VerifyCounselorDto verifyDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                var (success, message) = await _counselorService.VerifyCounselorAsync(counselorId, currentUserId);

                if (success)
                {
                    return Ok(new { message });
                }

                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying counselor {CounselorId}", counselorId);
                return StatusCode(500, new { message = "An error occurred while verifying the counselor" });
            }
        }

        /// <summary>
        /// Unverify a counselor - Admin only
        /// </summary>
        [HttpPost("{counselorId}/unverify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnverifyCounselor(Guid counselorId, [FromBody] VerifyCounselorDto unverifyDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                var (success, message) = await _counselorService.UnverifyCounselorAsync(counselorId, currentUserId);

                if (success)
                {
                    return Ok(new { message });
                }

                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unverifying counselor {CounselorId}", counselorId);
                return StatusCode(500, new { message = "An error occurred while unverifying the counselor" });
            }
        }

        /// <summary>
        /// Set counselor general availability - Admin or Counselor themselves
        /// </summary>
        [HttpPost("{counselorId}/set-general-availability")]
        [Authorize(Roles = "Admin,Counselor")]
        public async Task<IActionResult> SetGeneralAvailability(Guid counselorId, [FromBody] SetCounselorGeneralAvailabilityDto availabilityDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                // Check if user is admin or the counselor themselves
                var userRoles = GetUserRoles();
                if (!userRoles.Contains("Admin"))
                {
                    // If not admin, check if they are the counselor themselves
                    var counselor = await _counselorService.GetCounselorAsync(counselorId);
                    if (counselor == null || counselor.UserId != currentUserId)
                    {
                        return Forbid("You can only modify your own availability");
                    }
                }

                var (success, message) = await _counselorService.SetCounselorGeneralAvailabilityAsync(
                    counselorId, availabilityDto.IsAvailable, currentUserId);

                if (success)
                {
                    return Ok(new { message });
                }

                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting general availability for counselor {CounselorId}", counselorId);
                return StatusCode(500, new { message = "An error occurred while setting counselor availability" });
            }
        }

        /// <summary>
        /// Update counselor profile - Admin or Counselor themselves
        /// </summary>
        [HttpPut("{counselorId}/profile")]
        [Authorize(Roles = "Admin,Counselor")]
        public async Task<IActionResult> UpdateProfile(Guid counselorId, [FromBody] UpdateCounselorDto updateDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                // Check if user is admin or the counselor themselves
                var userRoles = GetUserRoles();
                if (!userRoles.Contains("Admin"))
                {
                    var counselor = await _counselorService.GetCounselorAsync(counselorId);
                    if (counselor == null || counselor.UserId != currentUserId)
                    {
                        return Forbid("You can only modify your own profile");
                    }
                }

                var (success, message) = await _counselorService.UpdateCounselorProfileAsync(
                    counselorId, updateDto, currentUserId);

                if (success)
                {
                    return Ok(new { message });
                }

                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating counselor profile {CounselorId}", counselorId);
                return StatusCode(500, new { message = "An error occurred while updating counselor profile" });
            }
        }

        /// <summary>
        /// Get counselor details
        /// </summary>
        [HttpGet("{counselorId}")]
        public async Task<IActionResult> GetCounselor(Guid counselorId)
        {
            try
            {
                var counselor = await _counselorService.GetCounselorAsync(counselorId);
                if (counselor == null)
                {
                    return NotFound(new { message = "Counselor not found" });
                }

                return Ok(counselor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor {CounselorId}", counselorId);
                return StatusCode(500, new { message = "An error occurred while retrieving counselor details" });
            }
        }

        /// <summary>
        /// Get counselor details by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCounselorByUserId(Guid userId)
        {
            try
            {
                var counselor = await _counselorService.GetCounselorByUserIdAsync(userId);
                if (counselor == null)
                {
                    return NotFound(new { message = "Counselor not found by userId" });
                }

                return Ok(counselor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor by userId {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving counselor details by userId" });
            }
        }

        /// <summary>
        /// Get all counselors with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCounselors([FromQuery] CounselorFilterDto? filter = null)
        {
            try
            {
                var counselors = await _counselorService.GetCounselorsAsync(filter);
                return Ok(counselors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselors");
                return StatusCode(500, new { message = "An error occurred while retrieving counselors" });
            }
        }

        /// <summary>
        /// Get counselor status - useful for debugging
        /// </summary>
        [HttpGet("{counselorId}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetCounselorStatus(Guid counselorId)
        {
            try
            {
                var status = await _counselorService.GetCounselorStatusAsync(counselorId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor status {CounselorId}", counselorId);
                return StatusCode(500, new { message = "An error occurred while retrieving counselor status" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private List<string> GetUserRoles()
        {
            return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        /// <summary>
        /// Get all appointments for a specific counselor - Admin or Counselor themselves
        /// FIXED: Remove pagination from response structure
        /// </summary>
        [HttpGet("{counselorId}/appointments")]
        [Authorize(Roles = "Admin,Counselor")]
        public async Task<IActionResult> GetCounselorAppointments(Guid counselorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                {
                    return Unauthorized("Invalid user token");
                }

                // Check if user is admin or the counselor themselves
                var userRoles = GetUserRoles();
                if (!userRoles.Contains("Admin"))
                {
                    // If not admin, check if they are the counselor themselves
                    var counselor = await _counselorService.GetCounselorAsync(counselorId);
                    if (counselor == null)
                    {
                        return NotFound(new { message = "Counselor not found" });
                    }

                    if (counselor.UserId != currentUserId)
                    {
                        return Forbid("You can only view your own appointments");
                    }
                }

                // Validate pagination parameters
                if (page < 1)
                {
                    return BadRequest(new { message = "Page number must be greater than 0" });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new { message = "Page size must be between 1 and 100" });
                }

                var appointments = await _counselorService.GetCounselorAppointmentsAsync(counselorId, page, pageSize);

                // FIXED: Return only the data array without pagination wrapper
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointments for counselor {CounselorId}", counselorId);
                return StatusCode(500, new { message = "An error occurred while retrieving counselor appointments" });
            }
        }
    }
}
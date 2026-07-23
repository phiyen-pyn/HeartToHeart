using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            if (currentUserId != userId && !User.IsInRole("Admin"))
                return Forbidden(new { Message = "You are not authorized to view this user's information" });

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        [HttpGet("email")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userService.GetAllUsersAsync(page, pageSize);
            var totalCount = await _userService.GetTotalUsersCountAsync();
            return Ok(new
            {
                Users = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var result = await _userService.UpdateUserProfileAsync(userId, updateDto);
            if (result)
                return Ok(new { Message = "Profile updated successfully" });

            return BadRequest(new { Message = "Failed to update profile" });
        }

        [HttpPut("{userId}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(Guid userId)
        {
            var result = await _userService.DeactivateUserAsync(userId);
            if (result)
                return Ok(new { Message = "User deactivated successfully" });

            return BadRequest(new { Message = "Failed to deactivate user" });
        }

        [HttpPut("{userId}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateUser(Guid userId)
        {
            var result = await _userService.ActivateUserAsync(userId);
            if (result)
                return Ok(new { Message = "User activated successfully" });

            return BadRequest(new { Message = "Failed to activate user" });
        }

        [HttpGet("role/{roleName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            var users = await _userService.GetUsersByRoleAsync(roleName);
            return Ok(users);
        }

        [HttpGet("count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalUsersCount()
        {
            var count = await _userService.GetTotalUsersCountAsync();
            return Ok(new { TotalCount = count });
        }

        private IActionResult Forbidden(object value)
        {
            return StatusCode(403, value);
        }
    }
}

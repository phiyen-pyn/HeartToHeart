using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var roles = User.Identity?.IsAuthenticated == true
                ? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                : [Roles.Guest];
            var subs = await _subscriptionService.GetAllAsync(roles, page, pageSize);
            var totalCount = await _subscriptionService.GetTotalCountAsync();
            return Ok(new
            {
                Subscriptions = subs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("my-subs")]
        public async Task<IActionResult> GetAllByUserIdAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var subs = await _subscriptionService.GetByUserIdAsync(userId, page, pageSize);
            var totalCount = await _subscriptionService.GetTotalByUserIdCountAsync(userId);
            return Ok(new
            {
                Subscriptions = subs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{subId}")]
        public async Task<IActionResult> GetByIdAsync(Guid subId)
        {
            var roles = User.Identity?.IsAuthenticated == true
                ? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                : [Roles.Guest];
            var sub = await _subscriptionService.GetByIdAsync(roles, subId);
            if (sub == null)
                return NotFound(new { Message = "Subscription not found" });
            return Ok(sub);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSubscriptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var subscriptionDto = new SubscriptionDto
            {
                UserId = userId,
                PlanName = dto.PlanName,
                Amount = dto.Amount,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(dto.DurationDays),
                Status = SubscriptionStatus.Pending,
            };
            var result = await _subscriptionService.AddAsync(subscriptionDto);
            return Ok(result);
        }

        [HttpPut("{subId}")]
        public async Task<IActionResult> UpdateAsync(Guid subId, [FromBody] UpdateSubscriptionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var subscriptionDto = new SubscriptionDto
            {
                Status = dto.Status,
                UpdatedAt = DateTime.UtcNow,
            };
            var result = await _subscriptionService.UpdateAsync(subId, subscriptionDto);
            return Ok(result);
        }

        [HttpDelete("{subId}")]
        public async Task<IActionResult> DeleteAsync(Guid subId)
        {
            var result = await _subscriptionService.DeleteAsync(subId);
            if (result)
                return Ok(new { Message = "Sub deleted successfully" });
            return BadRequest(new { Message = "Sub deleted failed" });
        }
    }
}

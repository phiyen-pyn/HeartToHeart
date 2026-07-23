using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.BLL.Services;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnonymousPostController : ControllerBase
    {
        private readonly IAnonymousPostService _postService;

        public AnonymousPostController(IAnonymousPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostById(Guid postId)
        {
            var roles = User.Identity?.IsAuthenticated == true
                ? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                : [Roles.Guest];
            var post = await _postService.GetByIdAsync(roles, postId);
            if (post == null)
                return NotFound(new { Message = "Post not found" });
            return Ok(post);
        }

        [HttpGet]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var roles = User.Identity?.IsAuthenticated == true
                ? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                : [Roles.Guest];
            var posts = await _postService.GetAllAsync(roles, page, pageSize);
            var totalCount = await _postService.GetTotalCountAsync();
            return Ok(new
            {
                Posts = posts,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("my-post")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllByUserIdAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var posts = await _postService.GetAllByUserIdAsync(userId, page, pageSize);
            var totalCount = await _postService.GetTotalByUserIdCountAsync(userId);
            return Ok(new
            {
                Posts = posts,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("report")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> GetReportedPostsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var roles = User.Identity?.IsAuthenticated == true
                ? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                : [Roles.Guest];
            var posts = await _postService.GetReportedPostsAsync(roles, page, pageSize);
            var totalCount = await _postService.GetTotalReportedCountAsync();
            return Ok(new
            {
                Posts = posts,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("my-report")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> GetReportedPostsByUserIdAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var posts = await _postService.GetReportedPostsByUserIdAsync(userId, page, pageSize);
            var totalCount = await _postService.GetTotalReportedByUserIdCountAsync(userId);
            return Ok(new
            {
                Posts = posts,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpPost]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] CreateAnonymousPostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var anonymousPostDto = new AnonymousPostDto
            {
                UserId = userId,
                Content = dto.Content,
                IsReported = false,
            };
            var result = await _postService.CreateAsync(anonymousPostDto);
            return Ok(result);
        }

        [HttpPut("{postId}")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> UpdateAsync(Guid postId, [FromBody] UpdateAnonymousPostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var anonymousPostDto = new AnonymousPostDto
            {
                Content = dto.Content,
                IsReported = dto.IsReported,
            };
            var result = await _postService.UpdateAsync(postId, anonymousPostDto);
            return Ok(result);
        }

        [HttpDelete("{postId}")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> DeleteAsync(Guid postId)
        {
            var result = await _postService.DeleteAsync(postId);
            if (result)
                return Ok(new { Message = "Post deleted successfully" });
            return BadRequest(new { Message = "Post deleted failed" });
        }

        [HttpGet("count")]
        [AllowAnonymous]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalPostsCount()
        {
            var count = await _postService.GetTotalCountAsync();
            return Ok(new { TotalCount = count });
        }
    }
}

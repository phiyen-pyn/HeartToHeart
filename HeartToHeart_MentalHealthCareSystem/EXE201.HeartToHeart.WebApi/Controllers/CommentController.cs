using EXE201.HeartToHeart.BLL.IServices;
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
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{commentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentById(Guid commentId)
        {
            var roles = User.Identity?.IsAuthenticated == true
                ? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                : [Roles.Guest];
            var comment = await _commentService.GetByIdAsync(roles, commentId);
            if (comment == null)
                return NotFound(new { Message = "Comment not found" });
            return Ok(comment);
        }

        [HttpGet("post/{postId}")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> GetAllByPostIdAsync(Guid postId)
        {
            var roles = User.Identity?.IsAuthenticated == true
                ? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                : [Roles.Guest];
            var comments = await _commentService.GetAllByPostIdAsync(roles, postId);
            return Ok(comments);
        }

        [HttpPost]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var commentDto = new CommentDto
            {
                PostId = dto.PostId,
                UserId = userId,
                Content = dto.Content,
            };
            var result = await _commentService.CreateAsync(commentDto);
            return Ok(result);
        }

        [HttpPut("{commentId}")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> UpdateAsync(Guid commentId, [FromBody] UpdateCommentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var commentDto = new CommentDto
            {
                UserId = userId,
                Content = dto.Content,
            };
            var result = await _commentService.UpdateAsync(commentId, commentDto);
            return Ok(result);
        }

        [HttpDelete("{commentId}")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> DeleteAsync(Guid commentId)
        {
            var result = await _commentService.DeleteAsync(commentId);
            if (result)
                return Ok(new { Message = "Comment deleted successfully" });
            return BadRequest(new { Message = "Comment deleted failed" });
        }
    }
}

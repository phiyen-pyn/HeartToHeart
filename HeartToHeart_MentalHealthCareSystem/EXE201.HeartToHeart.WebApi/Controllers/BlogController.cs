using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        #region Blog CRUD Operations

        [HttpGet("{blogId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlogById(Guid blogId)
        {
            var userId = User.Identity.IsAuthenticated ?
                Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString()) :
                (Guid?)null;

            var blog = await _blogService.GetBlogByIdAsync(blogId, userId);
            if (blog == null)
                return NotFound(new { Message = "Blog not found" });

            // Increment view count
            await _blogService.IncrementViewCountAsync(blogId);

            return Ok(blog);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _blogService.GetAllBlogsAsync(page, pageSize);
            return Ok(blogs);
        }

        [HttpGet("published")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _blogService.GetPublishedBlogsAsync(page, pageSize);
            return Ok(blogs);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetBlogsByUserId(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _blogService.GetBlogsByUserIdAsync(userId, page, pageSize);
            return Ok(blogs);
        }

        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlogsByCategory(string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _blogService.GetBlogsByCategoryAsync(category, page, pageSize);
            return Ok(blogs);
        }

        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeaturedBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _blogService.GetFeaturedBlogsAsync(page, pageSize);
            return Ok(blogs);
        }

        [HttpGet("premium")]
        [Authorize]
        public async Task<IActionResult> GetPremiumBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _blogService.GetPremiumBlogsAsync(page, pageSize);
            return Ok(blogs);
        }

        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopularBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _blogService.GetPopularBlogsAsync(page, pageSize);
            return Ok(blogs);
        }

        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var blogs = await _blogService.GetRecentBlogsAsync(page, pageSize);
            return Ok(blogs);
        }

        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchBlogs([FromBody] BlogSearchDto searchDto)
        {
            var blogs = await _blogService.SearchBlogsAsync(searchDto);
            return Ok(blogs);
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CreateBlog([FromBody] CreateBlogDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var blog = await _blogService.CreateBlogAsync(userId, createDto);

            if (blog == null)
                return BadRequest(new { Message = "Failed to create blog" });

            return CreatedAtAction(nameof(GetBlogById), new { blogId = blog.Id }, blog);
        }

        [HttpPut("{blogId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateBlog(Guid blogId, [FromBody] UpdateBlogDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _blogService.UpdateBlogAsync(blogId, updateDto, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to update blog or unauthorized access" });

            return Ok(new { Message = "Blog updated successfully" });
        }

        [HttpPatch("{blogId}/publish")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> PublishBlog(Guid blogId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _blogService.PublishBlogAsync(blogId, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to publish blog or unauthorized access" });

            return Ok(new { Message = "Blog published successfully" });
        }

        [HttpPatch("{blogId}/unpublish")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UnpublishBlog(Guid blogId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _blogService.UnpublishBlogAsync(blogId, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to unpublish blog or unauthorized access" });

            return Ok(new { Message = "Blog unpublished successfully" });
        }

        [HttpDelete("{blogId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> DeleteBlog(Guid blogId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _blogService.DeleteBlogAsync(blogId, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to delete blog or unauthorized access" });

            return Ok(new { Message = "Blog deleted successfully" });
        }

        #endregion

        #region Blog Comment Operations

        [HttpGet("{blogId}/comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentsByBlogId(Guid blogId)
        {
            var comments = await _blogService.GetCommentsByBlogIdAsync(blogId);
            return Ok(comments);
        }

        [HttpGet("comments/{commentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentById(Guid commentId)
        {
            var comment = await _blogService.GetCommentByIdAsync(commentId);
            if (comment == null)
                return NotFound(new { Message = "Comment not found" });

            return Ok(comment);
        }

        [HttpGet("user/{userId}/comments")]
        [Authorize]
        public async Task<IActionResult> GetCommentsByUserId(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var comments = await _blogService.GetCommentsByUserIdAsync(userId, page, pageSize);
            return Ok(comments);
        }

        [HttpPost("comments")]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] CreateBlogCommentDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var comment = await _blogService.CreateCommentAsync(userId, createDto);

            if (comment == null)
                return BadRequest(new { Message = "Failed to create comment" });

            return CreatedAtAction(nameof(GetCommentById), new { commentId = comment.Id }, comment);
        }

        [HttpPut("comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateBlogCommentDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _blogService.UpdateCommentAsync(commentId, updateDto, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to update comment or unauthorized access" });

            return Ok(new { Message = "Comment updated successfully" });
        }

        [HttpDelete("comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _blogService.DeleteCommentAsync(commentId, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to delete comment or unauthorized access" });

            return Ok(new { Message = "Comment deleted successfully" });
        }

        [HttpPatch("comments/{commentId}/approve")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> ApproveComment(Guid commentId)
        {
            var result = await _blogService.ApproveCommentAsync(commentId);

            if (!result)
                return BadRequest(new { Message = "Failed to approve comment" });

            return Ok(new { Message = "Comment approved successfully" });
        }

        [HttpPatch("comments/{commentId}/reject")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> RejectComment(Guid commentId)
        {
            var result = await _blogService.RejectCommentAsync(commentId);

            if (!result)
                return BadRequest(new { Message = "Failed to reject comment" });

            return Ok(new { Message = "Comment rejected successfully" });
        }

        #endregion

        #region Blog Like Operations

        [HttpPost("{blogId}/like")]
        [Authorize]
        public async Task<IActionResult> LikeBlog(Guid blogId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _blogService.LikeBlogAsync(blogId, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to like blog" });

            return Ok(new { Message = "Blog liked successfully" });
        }

        [HttpDelete("{blogId}/like")]
        [Authorize]
        public async Task<IActionResult> UnlikeBlog(Guid blogId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _blogService.UnlikeBlogAsync(blogId, userId);

            if (!result)
                return BadRequest(new { Message = "Failed to unlike blog" });

            return Ok(new { Message = "Blog unliked successfully" });
        }

        [HttpGet("{blogId}/likes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlogLikes(Guid blogId)
        {
            var likes = await _blogService.GetBlogLikesAsync(blogId);
            return Ok(likes);
        }

        [HttpGet("{blogId}/liked")]
        [Authorize]
        public async Task<IActionResult> CheckIfUserLikedBlog(Guid blogId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var hasLiked = await _blogService.UserHasLikedBlogAsync(blogId, userId);

            return Ok(new { HasLiked = hasLiked });
        }

        #endregion

        #region Tag Operations

        [HttpGet("tags")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await _blogService.GetAllTagsAsync();
            return Ok(tags);
        }

        [HttpGet("tags/popular")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopularTags([FromQuery] int count = 10)
        {
            var tags = await _blogService.GetPopularTagsAsync(count);
            return Ok(tags);
        }

        [HttpGet("tags/{tagId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTagById(Guid tagId)
        {
            var tag = await _blogService.GetTagByIdAsync(tagId);
            if (tag == null)
                return NotFound(new { Message = "Tag not found" });

            return Ok(tag);
        }

        [HttpGet("tags/name/{name}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTagByName(string name)
        {
            var tag = await _blogService.GetTagByNameAsync(name);
            if (tag == null)
                return NotFound(new { Message = "Tag not found" });

            return Ok(tag);
        }

        [HttpGet("{blogId}/tags")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTagsByBlogId(Guid blogId)
        {
            var tags = await _blogService.GetTagsByBlogIdAsync(blogId);
            return Ok(tags);
        }

        [HttpPost("tags")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tag = await _blogService.CreateTagAsync(createDto);

            if (tag == null)
                return BadRequest(new { Message = "Failed to create tag or tag already exists" });

            return CreatedAtAction(nameof(GetTagById), new { tagId = tag.Id }, tag);
        }

        [HttpPut("tags/{tagId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateTag(Guid tagId, [FromBody] UpdateTagDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _blogService.UpdateTagAsync(tagId, updateDto);

            if (!result)
                return BadRequest(new { Message = "Failed to update tag" });

            return Ok(new { Message = "Tag updated successfully" });
        }

        [HttpDelete("tags/{tagId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> DeleteTag(Guid tagId)
        {
            var result = await _blogService.DeleteTagAsync(tagId);

            if (!result)
                return BadRequest(new { Message = "Failed to delete tag" });

            return Ok(new { Message = "Tag deleted successfully" });
        }

        #endregion

        #region Statistics and Analytics

        [HttpGet("statistics")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetBlogStatistics()
        {
            var stats = await _blogService.GetBlogStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDistinctCategories()
        {
            var categories = await _blogService.GetDistinctCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("counts/total")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalBlogsCount()
        {
            var count = await _blogService.GetTotalBlogsCountAsync();
            return Ok(new { TotalBlogs = count });
        }

        [HttpGet("counts/published")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedBlogsCount()
        {
            var count = await _blogService.GetPublishedBlogsCountAsync();
            return Ok(new { PublishedBlogs = count });
        }

        [HttpGet("counts/user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetBlogsByUserCount(Guid userId)
        {
            var count = await _blogService.GetBlogsByUserCountAsync(userId);
            return Ok(new { UserBlogs = count });
        }

        [HttpGet("counts/category/{category}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlogsByCategoryCount(string category)
        {
            var count = await _blogService.GetBlogsByCategoryCountAsync(category);
            return Ok(new { CategoryBlogs = count });
        }

        [HttpGet("counts/featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeaturedBlogsCount()
        {
            var count = await _blogService.GetFeaturedBlogsCountAsync();
            return Ok(new { FeaturedBlogs = count });
        }

        [HttpGet("counts/premium")]
        [Authorize]
        public async Task<IActionResult> GetPremiumBlogsCount()
        {
            var count = await _blogService.GetPremiumBlogsCountAsync();
            return Ok(new { PremiumBlogs = count });
        }

        [HttpGet("{blogId}/counts/comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentsByBlogCount(Guid blogId)
        {
            var count = await _blogService.GetCommentsByBlogCountAsync(blogId);
            return Ok(new { CommentsCount = count });
        }

        [HttpGet("{blogId}/counts/likes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLikesByBlogCount(Guid blogId)
        {
            var count = await _blogService.GetLikesByBlogCountAsync(blogId);
            return Ok(new { LikesCount = count });
        }

        #endregion
    }
}

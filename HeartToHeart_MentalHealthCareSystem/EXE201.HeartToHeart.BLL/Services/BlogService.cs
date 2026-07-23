using EXE201.HeartToHeart.BLL.IServices;
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
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly ILogger<BlogService> _logger;

        public BlogService(IBlogRepository blogRepository, ILogger<BlogService> logger)
        {
            _blogRepository = blogRepository;
            _logger = logger;
        }

        // Blog CRUD Operations
        public async Task<BlogDto?> GetBlogByIdAsync(Guid blogId, Guid? currentUserId = null)
        {
            try
            {
                var blog = await _blogRepository.GetBlogByIdAsync(blogId);
                if (blog == null) return null;

                var blogDto = MapBlogToDto(blog);

                if (currentUserId.HasValue)
                {
                    blogDto.IsLikedByCurrentUser = await _blogRepository.UserHasLikedBlogAsync(blogId, currentUserId.Value);
                }

                return blogDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog by ID: {BlogId}", blogId);
                return null;
            }
        }

        public async Task<IEnumerable<BlogDto>> GetAllBlogsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var blogs = await _blogRepository.GetAllBlogsAsync(page, pageSize);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blogs");
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<IEnumerable<BlogDto>> GetPublishedBlogsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var blogs = await _blogRepository.GetPublishedBlogsAsync(page, pageSize);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published blogs");
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<IEnumerable<BlogDto>> GetBlogsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var blogs = await _blogRepository.GetBlogsByUserIdAsync(userId, page, pageSize);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blogs by user ID: {UserId}", userId);
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<IEnumerable<BlogDto>> GetBlogsByCategoryAsync(string category, int page = 1, int pageSize = 10)
        {
            try
            {
                var blogs = await _blogRepository.GetBlogsByCategoryAsync(category, page, pageSize);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blogs by category: {Category}", category);
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<IEnumerable<BlogDto>> GetFeaturedBlogsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var blogs = await _blogRepository.GetFeaturedBlogsAsync(page, pageSize);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured blogs");
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<IEnumerable<BlogDto>> GetPremiumBlogsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var blogs = await _blogRepository.GetPremiumBlogsAsync(page, pageSize);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting premium blogs");
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<IEnumerable<BlogDto>> SearchBlogsAsync(BlogSearchDto searchDto)
        {
            try
            {
                var blogs = await _blogRepository.SearchBlogsAsync(searchDto);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching blogs");
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<IEnumerable<BlogDto>> GetPopularBlogsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var blogs = await _blogRepository.GetPopularBlogsAsync(page, pageSize);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular blogs");
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<IEnumerable<BlogDto>> GetRecentBlogsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var blogs = await _blogRepository.GetRecentBlogsAsync(page, pageSize);
                return blogs.Select(MapBlogToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent blogs");
                return Enumerable.Empty<BlogDto>();
            }
        }

        public async Task<BlogDto?> CreateBlogAsync(Guid userId, CreateBlogDto createDto)
        {
            try
            {
                var blog = new Blog
                {
                    Title = createDto.Title,
                    Content = createDto.Content,
                    Summary = createDto.Summary,
                    FeaturedImage = createDto.FeaturedImage,
                    Category = createDto.Category,
                    IsPublished = createDto.IsPublished,
                    IsFeatured = createDto.IsFeatured,
                    IsPremium = createDto.IsPremium,
                    UserId = userId,
                    PublishedAt = createDto.IsPublished ? DateTime.UtcNow : null,
                    CreatedBy = userId.ToString(),
                    UpdatedBy = userId.ToString()
                };

                var success = await _blogRepository.CreateBlogAsync(blog);
                if (!success) return null;

                // Handle tags
                await ProcessBlogTagsAsync(blog.Id, createDto.Tags);

                return await GetBlogByIdAsync(blog.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog for user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateBlogAsync(Guid blogId, UpdateBlogDto updateDto, Guid userId)
        {
            try
            {
                var blog = await _blogRepository.GetBlogByIdAsync(blogId);
                if (blog == null || blog.UserId != userId) return false;

                if (!string.IsNullOrEmpty(updateDto.Title))
                    blog.Title = updateDto.Title;
                if (!string.IsNullOrEmpty(updateDto.Content))
                    blog.Content = updateDto.Content;
                if (updateDto.Summary != null)
                    blog.Summary = updateDto.Summary;
                if (updateDto.FeaturedImage != null)
                    blog.FeaturedImage = updateDto.FeaturedImage;
                if (updateDto.Category != null)
                    blog.Category = updateDto.Category;
                if (updateDto.IsPublished.HasValue)
                {
                    blog.IsPublished = updateDto.IsPublished.Value;
                    if (updateDto.IsPublished.Value && blog.PublishedAt == null)
                        blog.PublishedAt = DateTime.UtcNow;
                }
                if (updateDto.IsFeatured.HasValue)
                    blog.IsFeatured = updateDto.IsFeatured.Value;
                if (updateDto.IsPremium.HasValue)
                    blog.IsPremium = updateDto.IsPremium.Value;

                blog.UpdatedAt = DateTime.UtcNow;
                blog.UpdatedBy = userId.ToString();

                var success = await _blogRepository.UpdateBlogAsync(blog);

                // Handle tags if provided
                if (updateDto.Tags != null && success)
                {
                    await _blogRepository.RemoveAllBlogTagsAsync(blogId);
                    await ProcessBlogTagsAsync(blogId, updateDto.Tags);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog: {BlogId}", blogId);
                return false;
            }
        }

        public async Task<bool> PublishBlogAsync(Guid blogId, Guid userId)
        {
            try
            {
                var blog = await _blogRepository.GetBlogByIdAsync(blogId);
                if (blog == null || blog.UserId != userId) return false;

                blog.IsPublished = true;
                blog.PublishedAt = DateTime.UtcNow;
                blog.UpdatedAt = DateTime.UtcNow;
                blog.UpdatedBy = userId.ToString();

                return await _blogRepository.UpdateBlogAsync(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing blog: {BlogId}", blogId);
                return false;
            }
        }

        public async Task<bool> UnpublishBlogAsync(Guid blogId, Guid userId)
        {
            try
            {
                var blog = await _blogRepository.GetBlogByIdAsync(blogId);
                if (blog == null || blog.UserId != userId) return false;

                blog.IsPublished = false;
                blog.UpdatedAt = DateTime.UtcNow;
                blog.UpdatedBy = userId.ToString();

                return await _blogRepository.UpdateBlogAsync(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing blog: {BlogId}", blogId);
                return false;
            }
        }

        public async Task<bool> DeleteBlogAsync(Guid blogId, Guid userId)
        {
            try
            {
                var blog = await _blogRepository.GetBlogByIdAsync(blogId);
                if (blog == null || blog.UserId != userId) return false;

                return await _blogRepository.DeleteBlogAsync(blogId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog: {BlogId}", blogId);
                return false;
            }
        }

        public async Task<bool> IncrementViewCountAsync(Guid blogId)
        {
            try
            {
                return await _blogRepository.IncrementViewCountAsync(blogId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing view count for blog: {BlogId}", blogId);
                return false;
            }
        }

        // Blog Comment Operations
        public async Task<BlogCommentDto?> GetCommentByIdAsync(Guid commentId)
        {
            try
            {
                var comment = await _blogRepository.GetCommentByIdAsync(commentId);
                return comment != null ? MapCommentToDto(comment) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment by ID: {CommentId}", commentId);
                return null;
            }
        }

        public async Task<IEnumerable<BlogCommentDto>> GetCommentsByBlogIdAsync(Guid blogId)
        {
            try
            {
                var comments = await _blogRepository.GetCommentsByBlogIdAsync(blogId);
                return comments.Select(MapCommentToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for blog: {BlogId}", blogId);
                return Enumerable.Empty<BlogCommentDto>();
            }
        }

        public async Task<IEnumerable<BlogCommentDto>> GetCommentsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var comments = await _blogRepository.GetCommentsByUserIdAsync(userId, page, pageSize);
                return comments.Select(MapCommentToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for user: {UserId}", userId);
                return Enumerable.Empty<BlogCommentDto>();
            }
        }

        public async Task<BlogCommentDto?> CreateCommentAsync(Guid userId, CreateBlogCommentDto createDto)
        {
            try
            {
                var comment = new BlogComment
                {
                    BlogId = createDto.BlogId,
                    UserId = userId,
                    Content = createDto.Content,
                    ParentCommentId = createDto.ParentCommentId,
                    IsApproved = true, // Auto-approve for now
                    CreatedBy = userId.ToString(),
                    UpdatedBy = userId.ToString()
                };

                var success = await _blogRepository.CreateCommentAsync(comment);
                if (!success) return null;

                return await GetCommentByIdAsync(comment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateCommentAsync(Guid commentId, UpdateBlogCommentDto updateDto, Guid userId)
        {
            try
            {
                var comment = await _blogRepository.GetCommentByIdAsync(commentId);
                if (comment == null || comment.UserId != userId) return false;

                comment.Content = updateDto.Content;
                comment.UpdatedAt = DateTime.UtcNow;
                comment.UpdatedBy = userId.ToString();

                return await _blogRepository.UpdateCommentAsync(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment: {CommentId}", commentId);
                return false;
            }
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            try
            {
                var comment = await _blogRepository.GetCommentByIdAsync(commentId);
                if (comment == null || comment.UserId != userId) return false;

                return await _blogRepository.DeleteCommentAsync(commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment: {CommentId}", commentId);
                return false;
            }
        }

        public async Task<bool> ApproveCommentAsync(Guid commentId)
        {
            try
            {
                return await _blogRepository.ApproveCommentAsync(commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving comment: {CommentId}", commentId);
                return false;
            }
        }

        public async Task<bool> RejectCommentAsync(Guid commentId)
        {
            try
            {
                return await _blogRepository.RejectCommentAsync(commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting comment: {CommentId}", commentId);
                return false;
            }
        }

        // Blog Like Operations
        public async Task<bool> LikeBlogAsync(Guid blogId, Guid userId)
        {
            try
            {
                var existingLike = await _blogRepository.GetBlogLikeAsync(blogId, userId);
                if (existingLike != null) return true; // Already liked

                var blogLike = new BlogLike
                {
                    BlogId = blogId,
                    UserId = userId
                };

                return await _blogRepository.CreateBlogLikeAsync(blogLike);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking blog: {BlogId} by user: {UserId}", blogId, userId);
                return false;
            }
        }

        public async Task<bool> UnlikeBlogAsync(Guid blogId, Guid userId)
        {
            try
            {
                return await _blogRepository.DeleteBlogLikeAsync(blogId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking blog: {BlogId} by user: {UserId}", blogId, userId);
                return false;
            }
        }

        public async Task<bool> UserHasLikedBlogAsync(Guid blogId, Guid userId)
        {
            try
            {
                return await _blogRepository.UserHasLikedBlogAsync(blogId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user liked blog: {BlogId} by user: {UserId}", blogId, userId);
                return false;
            }
        }

        public async Task<IEnumerable<BlogLikeDto>> GetBlogLikesAsync(Guid blogId)
        {
            try
            {
                var likes = await _blogRepository.GetBlogLikesAsync(blogId);
                return likes.Select(MapLikeToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes for blog: {BlogId}", blogId);
                return Enumerable.Empty<BlogLikeDto>();
            }
        }

        // Tag Operations
        public async Task<TagDto?> GetTagByIdAsync(Guid tagId)
        {
            try
            {
                var tag = await _blogRepository.GetTagByIdAsync(tagId);
                return tag != null ? MapTagToDto(tag) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by ID: {TagId}", tagId);
                return null;
            }
        }

        public async Task<TagDto?> GetTagByNameAsync(string name)
        {
            try
            {
                var tag = await _blogRepository.GetTagByNameAsync(name);
                return tag != null ? MapTagToDto(tag) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by name: {Name}", name);
                return null;
            }
        }

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            try
            {
                var tags = await _blogRepository.GetAllTagsAsync();
                return tags.Select(MapTagToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tags");
                return Enumerable.Empty<TagDto>();
            }
        }

        public async Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10)
        {
            try
            {
                var tags = await _blogRepository.GetPopularTagsAsync(count);
                return tags.Select(MapTagToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular tags");
                return Enumerable.Empty<TagDto>();
            }
        }

        public async Task<IEnumerable<TagDto>> GetTagsByBlogIdAsync(Guid blogId)
        {
            try
            {
                var tags = await _blogRepository.GetTagsByBlogIdAsync(blogId);
                return tags.Select(MapTagToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags for blog: {BlogId}", blogId);
                return Enumerable.Empty<TagDto>();
            }
        }

        public async Task<TagDto?> CreateTagAsync(CreateTagDto createDto)
        {
            try
            {
                var existingTag = await _blogRepository.GetTagByNameAsync(createDto.Name);
                if (existingTag != null) return MapTagToDto(existingTag);

                var tag = new Tag
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Color = createDto.Color
                };

                var success = await _blogRepository.CreateTagAsync(tag);
                if (!success) return null;

                return MapTagToDto(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag: {Name}", createDto.Name);
                return null;
            }
        }

        public async Task<bool> UpdateTagAsync(Guid tagId, UpdateTagDto updateDto)
        {
            try
            {
                var tag = await _blogRepository.GetTagByIdAsync(tagId);
                if (tag == null) return false;

                if (!string.IsNullOrEmpty(updateDto.Name))
                    tag.Name = updateDto.Name;
                if (updateDto.Description != null)
                    tag.Description = updateDto.Description;
                if (updateDto.Color != null)
                    tag.Color = updateDto.Color;

                tag.UpdatedAt = DateTime.UtcNow;

                return await _blogRepository.UpdateTagAsync(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag: {TagId}", tagId);
                return false;
            }
        }

        public async Task<bool> DeleteTagAsync(Guid tagId)
        {
            try
            {
                return await _blogRepository.DeleteTagAsync(tagId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag: {TagId}", tagId);
                return false;
            }
        }

        // Statistics and Counts
        public async Task<int> GetTotalBlogsCountAsync()
        {
            try
            {
                return await _blogRepository.GetTotalBlogsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total blogs count");
                return 0;
            }
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _blogRepository.GetTotalCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total blogs count for {Year}-{Month}", year, month);
                return 0;
            }
        }

        public async Task<int> GetPublishedBlogsCountAsync()
        {
            try
            {
                return await _blogRepository.GetPublishedBlogsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published blogs count");
                return 0;
            }
        }

        public async Task<int> GetTotalPublishedCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _blogRepository.GetTotalPublishedCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total published blogs count for {Year}-{Month}", year, month);
                return 0;
            }
        }

        public async Task<int> GetBlogsByUserCountAsync(Guid userId)
        {
            try
            {
                return await _blogRepository.GetBlogsByUserCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blogs count by user: {UserId}", userId);
                return 0;
            }
        }

        public async Task<int> GetBlogsByCategoryCountAsync(string category)
        {
            try
            {
                return await _blogRepository.GetBlogsByCategoryCountAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blogs count by category: {Category}", category);
                return 0;
            }
        }

        public async Task<int> GetFeaturedBlogsCountAsync()
        {
            try
            {
                return await _blogRepository.GetFeaturedBlogsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured blogs count");
                return 0;
            }
        }

        public async Task<int> GetTotalFeaturedCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _blogRepository.GetTotalFeaturedCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total featured blogs count for {Year}-{Month}", year, month);
                return 0;
            }
        }

        public async Task<int> GetPremiumBlogsCountAsync()
        {
            try
            {
                return await _blogRepository.GetPremiumBlogsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting premium blogs count");
                return 0;
            }
        }

        public async Task<int> GetTotalPremiumCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _blogRepository.GetTotalPremiumCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total premium blogs count for {Year}-{Month}", year, month);
                return 0;
            }
        }

        public async Task<int> GetTotalCommentsCountAsync()
        {
            try
            {
                return await _blogRepository.GetTotalCommentsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total comments count");
                return 0;
            }
        }

        public async Task<int> GetCommentsByBlogCountAsync(Guid blogId)
        {
            try
            {
                return await _blogRepository.GetCommentsByBlogCountAsync(blogId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments count by blog: {BlogId}", blogId);
                return 0;
            }
        }

        public async Task<int> GetTotalLikesCountAsync()
        {
            try
            {
                return await _blogRepository.GetTotalLikesCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total likes count");
                return 0;
            }
        }

        public async Task<int> GetLikesByBlogCountAsync(Guid blogId)
        {
            try
            {
                return await _blogRepository.GetLikesByBlogCountAsync(blogId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes count by blog: {BlogId}", blogId);
                return 0;
            }
        }

        public async Task<int> GetTotalViewsCountAsync()
        {
            try
            {
                return await _blogRepository.GetTotalViewsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total views count");
                return 0;
            }
        }

        public async Task<int> GetTotalViewsCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _blogRepository.GetTotalViewsCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting views count for {Year}-{Month}", year, month);
                return 0;
            }
        }

        public async Task<IEnumerable<string>> GetDistinctCategoriesAsync()
        {
            try
            {
                return await _blogRepository.GetDistinctCategoriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distinct categories");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<BlogStatsDto> GetBlogStatisticsAsync()
        {
            try
            {
                return new BlogStatsDto
                {
                    TotalBlogs = await GetTotalBlogsCountAsync(),
                    PublishedBlogs = await GetPublishedBlogsCountAsync(),
                    DraftBlogs = await GetTotalBlogsCountAsync() - await GetPublishedBlogsCountAsync(),
                    FeaturedBlogs = await GetFeaturedBlogsCountAsync(),
                    PremiumBlogs = await GetPremiumBlogsCountAsync(),
                    TotalViews = await GetTotalViewsCountAsync(),
                    TotalLikes = await GetTotalLikesCountAsync(),
                    TotalComments = await GetTotalCommentsCountAsync(),
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog statistics");
                return new BlogStatsDto { GeneratedAt = DateTime.UtcNow };
            }
        }

        // Helper Methods
        private async Task ProcessBlogTagsAsync(Guid blogId, List<string> tagNames)
        {
            foreach (var tagName in tagNames)
            {
                var tag = await _blogRepository.GetTagByNameAsync(tagName);
                if (tag == null)
                {
                    // Create new tag
                    tag = new Tag { Name = tagName };
                    await _blogRepository.CreateTagAsync(tag);
                }

                // Add blog-tag association
                await _blogRepository.AddBlogTagAsync(blogId, tag.Id);

                // Increment tag usage count
                await _blogRepository.IncrementTagUsageAsync(tag.Id);
            }
        }

        private static BlogDto MapBlogToDto(Blog blog)
        {
            return new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Summary = blog.Summary,
                FeaturedImage = blog.FeaturedImage,
                Category = blog.Category,
                Tags = blog.BlogTags?.Select(bt => bt.Tag.Name).ToList() ?? new List<string>(),
                IsPublished = blog.IsPublished,
                PublishedAt = blog.PublishedAt,
                ViewCount = blog.ViewCount,
                LikeCount = blog.LikeCount,
                IsFeatured = blog.IsFeatured,
                IsPremium = blog.IsPremium,
                UserId = blog.UserId,
                AuthorName = blog.User?.FirstName + " " + blog.User?.LastName,
                AuthorEmail = blog.User?.Email,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                CreatedBy = blog.CreatedBy,
                UpdatedBy = blog.UpdatedBy,
                IsActive = blog.IsActive,
                Comments = blog.Comments?.Select(MapCommentToDto).ToList() ?? new List<BlogCommentDto>()
            };
        }

        private static BlogCommentDto MapCommentToDto(BlogComment comment)
        {
            return new BlogCommentDto
            {
                Id = comment.Id,
                BlogId = comment.BlogId,
                UserId = comment.UserId,
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
                IsApproved = comment.IsApproved,
                UserName = comment.User?.FirstName + " " + comment.User?.LastName,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Replies = comment.Replies?.Select(MapCommentToDto).ToList() ?? new List<BlogCommentDto>()
            };
        }

        private static BlogLikeDto MapLikeToDto(BlogLike like)
        {
            return new BlogLikeDto
            {
                Id = like.Id,
                BlogId = like.BlogId,
                UserId = like.UserId,
                UserName = like.User?.FirstName + " " + like.User?.LastName,
                CreatedAt = like.CreatedAt
            };
        }

        private static TagDto MapTagToDto(Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Description = tag.Description,
                Color = tag.Color,
                UsageCount = tag.UsageCount,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt,
                IsActive = tag.IsActive
            };
        }
    }
}

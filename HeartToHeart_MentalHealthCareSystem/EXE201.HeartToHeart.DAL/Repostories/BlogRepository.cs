using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Blog CRUD Operations
        public async Task<Blog?> GetBlogByIdAsync(Guid blogId)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Include(b => b.Comments.Where(c => c.IsApproved && c.ParentCommentId == null))
                .ThenInclude(c => c.User)
                .Include(b => b.Comments.Where(c => c.IsApproved && c.ParentCommentId == null))
                .ThenInclude(c => c.Replies.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
                .Include(b => b.Likes)
                .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(b => b.Id == blogId && b.IsActive);
        }

        public async Task<IEnumerable<Blog>> GetAllBlogsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetPublishedBlogsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive && b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetBlogsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive && b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetBlogsByCategoryAsync(string category, int page = 1, int pageSize = 10)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive && b.IsPublished && b.Category == category)
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetFeaturedBlogsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive && b.IsPublished && b.IsFeatured)
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetPremiumBlogsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive && b.IsPublished && b.IsPremium)
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> SearchBlogsAsync(BlogSearchDto searchDto)
        {
            var query = _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.Title))
                query = query.Where(b => b.Title.Contains(searchDto.Title));

            if (!string.IsNullOrEmpty(searchDto.Category))
                query = query.Where(b => b.Category == searchDto.Category);

            if (!string.IsNullOrEmpty(searchDto.Tag))
                query = query.Where(b => b.BlogTags.Any(bt => bt.Tag.Name == searchDto.Tag));

            if (searchDto.UserId.HasValue)
                query = query.Where(b => b.UserId == searchDto.UserId.Value);

            if (searchDto.IsPublished.HasValue)
                query = query.Where(b => b.IsPublished == searchDto.IsPublished.Value);

            if (searchDto.IsFeatured.HasValue)
                query = query.Where(b => b.IsFeatured == searchDto.IsFeatured.Value);

            if (searchDto.IsPremium.HasValue)
                query = query.Where(b => b.IsPremium == searchDto.IsPremium.Value);

            if (searchDto.FromDate.HasValue)
                query = query.Where(b => b.CreatedAt >= searchDto.FromDate.Value);

            if (searchDto.ToDate.HasValue)
                query = query.Where(b => b.CreatedAt <= searchDto.ToDate.Value);

            // Apply sorting
            query = searchDto.SortBy?.ToLower() switch
            {
                "viewcount" => searchDto.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(b => b.ViewCount)
                    : query.OrderByDescending(b => b.ViewCount),
                "likecount" => searchDto.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(b => b.LikeCount)
                    : query.OrderByDescending(b => b.LikeCount),
                "publishedat" => searchDto.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(b => b.PublishedAt)
                    : query.OrderByDescending(b => b.PublishedAt),
                _ => searchDto.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(b => b.CreatedAt)
                    : query.OrderByDescending(b => b.CreatedAt)
            };

            return await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetPopularBlogsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive && b.IsPublished)
                .OrderByDescending(b => b.ViewCount)
                .ThenByDescending(b => b.LikeCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetRecentBlogsAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogTags)
                .ThenInclude(bt => bt.Tag)
                .Where(b => b.IsActive && b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> CreateBlogAsync(Blog blog)
        {
            try
            {
                _context.Blogs.Add(blog);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBlogAsync(Blog blog)
        {
            try
            {
                _context.Blogs.Update(blog);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBlogAsync(Guid blogId)
        {
            try
            {
                var blog = await GetBlogByIdAsync(blogId);
                if (blog == null) return false;

                blog.IsActive = false;
                blog.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BlogExistsAsync(Guid blogId)
        {
            return await _context.Blogs.AnyAsync(b => b.Id == blogId && b.IsActive);
        }

        public async Task<bool> IncrementViewCountAsync(Guid blogId)
        {
            try
            {
                var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == blogId);
                if (blog == null) return false;

                blog.ViewCount++;
                blog.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateLikeCountAsync(Guid blogId)
        {
            try
            {
                var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == blogId);
                if (blog == null) return false;

                blog.LikeCount = await _context.BlogLikes.CountAsync(bl => bl.BlogId == blogId);
                blog.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        // Blog Comment Operations
        public async Task<BlogComment?> GetCommentByIdAsync(Guid commentId)
        {
            return await _context.BlogComments
                .Include(c => c.User)
                .Include(c => c.Blog)
                .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.IsActive);
        }

        public async Task<IEnumerable<BlogComment>> GetCommentsByBlogIdAsync(Guid blogId)
        {
            return await _context.BlogComments
                .Include(c => c.User)
                .Include(c => c.Replies.Where(r => r.IsActive && r.IsApproved))
                .ThenInclude(r => r.User)
                .Where(c => c.BlogId == blogId && c.IsActive && c.IsApproved && c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogComment>> GetCommentsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.BlogComments
                .Include(c => c.Blog)
                .Include(c => c.User)
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> CreateCommentAsync(BlogComment comment)
        {
            try
            {
                _context.BlogComments.Add(comment);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCommentAsync(BlogComment comment)
        {
            try
            {
                _context.BlogComments.Update(comment);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId)
        {
            try
            {
                var comment = await GetCommentByIdAsync(commentId);
                if (comment == null) return false;

                comment.IsActive = false;
                comment.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApproveCommentAsync(Guid commentId)
        {
            try
            {
                var comment = await _context.BlogComments.FirstOrDefaultAsync(c => c.Id == commentId);
                if (comment == null) return false;

                comment.IsApproved = true;
                comment.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectCommentAsync(Guid commentId)
        {
            try
            {
                var comment = await _context.BlogComments.FirstOrDefaultAsync(c => c.Id == commentId);
                if (comment == null) return false;

                comment.IsApproved = false;
                comment.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        // Blog Like Operations
        public async Task<BlogLike?> GetBlogLikeAsync(Guid blogId, Guid userId)
        {
            return await _context.BlogLikes
                .Include(bl => bl.Blog)
                .Include(bl => bl.User)
                .FirstOrDefaultAsync(bl => bl.BlogId == blogId && bl.UserId == userId && bl.IsActive);
        }

        public async Task<IEnumerable<BlogLike>> GetBlogLikesAsync(Guid blogId)
        {
            return await _context.BlogLikes
                .Include(bl => bl.User)
                .Where(bl => bl.BlogId == blogId && bl.IsActive)
                .OrderByDescending(bl => bl.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CreateBlogLikeAsync(BlogLike blogLike)
        {
            try
            {
                _context.BlogLikes.Add(blogLike);
                var result = await _context.SaveChangesAsync() > 0;
                if (result)
                {
                    await UpdateLikeCountAsync(blogLike.BlogId);
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBlogLikeAsync(Guid blogId, Guid userId)
        {
            try
            {
                var blogLike = await GetBlogLikeAsync(blogId, userId);
                if (blogLike == null) return false;

                _context.BlogLikes.Remove(blogLike);
                var result = await _context.SaveChangesAsync() > 0;
                if (result)
                {
                    await UpdateLikeCountAsync(blogId);
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UserHasLikedBlogAsync(Guid blogId, Guid userId)
        {
            return await _context.BlogLikes.AnyAsync(bl => bl.BlogId == blogId && bl.UserId == userId && bl.IsActive);
        }

        // Tag Operations
        public async Task<Tag?> GetTagByIdAsync(Guid tagId)
        {
            return await _context.Tags
                .Include(t => t.BlogTags)
                .ThenInclude(bt => bt.Blog)
                .FirstOrDefaultAsync(t => t.Id == tagId && t.IsActive);
        }

        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower() && t.IsActive);
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 10)
        {
            return await _context.Tags
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.UsageCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsByBlogIdAsync(Guid blogId)
        {
            return await _context.BlogTags
                .Include(bt => bt.Tag)
                .Where(bt => bt.BlogId == blogId && bt.Tag.IsActive)
                .Select(bt => bt.Tag)
                .ToListAsync();
        }

        public async Task<bool> CreateTagAsync(Tag tag)
        {
            try
            {
                _context.Tags.Add(tag);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            try
            {
                _context.Tags.Update(tag);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTagAsync(Guid tagId)
        {
            try
            {
                var tag = await GetTagByIdAsync(tagId);
                if (tag == null) return false;

                tag.IsActive = false;
                tag.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TagExistsAsync(string name)
        {
            return await _context.Tags.AnyAsync(t => t.Name.ToLower() == name.ToLower() && t.IsActive);
        }

        public async Task<bool> IncrementTagUsageAsync(Guid tagId)
        {
            try
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
                if (tag == null) return false;

                tag.UsageCount++;
                tag.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DecrementTagUsageAsync(Guid tagId)
        {
            try
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
                if (tag == null) return false;

                tag.UsageCount = Math.Max(0, tag.UsageCount - 1);
                tag.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        // Blog-Tag Association Operations
        public async Task<bool> AddBlogTagAsync(Guid blogId, Guid tagId)
        {
            try
            {
                var existingBlogTag = await _context.BlogTags
                    .FirstOrDefaultAsync(bt => bt.BlogId == blogId && bt.TagId == tagId);

                if (existingBlogTag != null) return true; // Already exists

                var blogTag = new BlogTag
                {
                    BlogId = blogId,
                    TagId = tagId
                };

                _context.BlogTags.Add(blogTag);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    await IncrementTagUsageAsync(tagId);
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveBlogTagAsync(Guid blogId, Guid tagId)
        {
            try
            {
                var blogTag = await _context.BlogTags
                    .FirstOrDefaultAsync(bt => bt.BlogId == blogId && bt.TagId == tagId);

                if (blogTag == null) return false;

                _context.BlogTags.Remove(blogTag);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    await DecrementTagUsageAsync(tagId);
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveAllBlogTagsAsync(Guid blogId)
        {
            try
            {
                var blogTags = await _context.BlogTags
                    .Where(bt => bt.BlogId == blogId)
                    .ToListAsync();

                if (!blogTags.Any()) return true;

                // Decrement usage count for all tags
                foreach (var blogTag in blogTags)
                {
                    await DecrementTagUsageAsync(blogTag.TagId);
                }

                _context.BlogTags.RemoveRange(blogTags);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        // Statistics and Counts
        public async Task<int> GetTotalBlogsCountAsync()
        {
            return await _context.Blogs.CountAsync(b => b.IsActive);
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.CreatedAt.Year == year && b.CreatedAt.Month == month);
        }

        public async Task<int> GetPublishedBlogsCountAsync()
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.IsPublished);
        }

        public async Task<int> GetTotalPublishedCountByMonthAsync(int year, int month)
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.IsPublished && b.CreatedAt.Year == year && b.CreatedAt.Month == month);
        }

        public async Task<int> GetBlogsByUserCountAsync(Guid userId)
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.UserId == userId);
        }

        public async Task<int> GetBlogsByCategoryCountAsync(string category)
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.IsPublished && b.Category == category);
        }

        public async Task<int> GetFeaturedBlogsCountAsync()
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.IsPublished && b.IsFeatured);
        }

        public async Task<int> GetTotalFeaturedCountByMonthAsync(int year, int month)
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.IsPublished && b.IsFeatured && b.CreatedAt.Year == year && b.CreatedAt.Month == month);
        }

        public async Task<int> GetPremiumBlogsCountAsync()
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.IsPublished && b.IsPremium);
        }

        public async Task<int> GetTotalPremiumCountByMonthAsync(int year, int month)
        {
            return await _context.Blogs.CountAsync(b => b.IsActive && b.IsPublished && b.IsPremium && b.CreatedAt.Year == year && b.CreatedAt.Month == month);
        }

        public async Task<int> GetTotalCommentsCountAsync()
        {
            return await _context.BlogComments.CountAsync(c => c.IsActive);
        }

        public async Task<int> GetCommentsByBlogCountAsync(Guid blogId)
        {
            return await _context.BlogComments.CountAsync(c => c.IsActive && c.BlogId == blogId);
        }

        public async Task<int> GetTotalLikesCountAsync()
        {
            return await _context.BlogLikes.CountAsync(l => l.IsActive);
        }

        public async Task<int> GetLikesByBlogCountAsync(Guid blogId)
        {
            return await _context.BlogLikes.CountAsync(l => l.IsActive && l.BlogId == blogId);
        }

        public async Task<int> GetTotalViewsCountAsync()
        {
            return await _context.Blogs.Where(b => b.IsActive).SumAsync(b => b.ViewCount);
        }

        public async Task<int> GetTotalViewsCountByMonthAsync(int year, int month)
        {
            return await _context.Blogs
                .Where(b => b.IsActive && b.CreatedAt.Year == year && b.CreatedAt.Month == month)
                .SumAsync(b => b.ViewCount);
        }

        public async Task<IEnumerable<string>> GetDistinctCategoriesAsync()
        {
            return await _context.Blogs
                .Where(b => b.IsActive && b.IsPublished && !string.IsNullOrEmpty(b.Category))
                .Select(b => b.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}

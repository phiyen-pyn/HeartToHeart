using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IBlogRepository
    {
        // Blog CRUD Operations
        Task<Blog?> GetBlogByIdAsync(Guid blogId);
        Task<IEnumerable<Blog>> GetAllBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Blog>> GetPublishedBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Blog>> GetBlogsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<Blog>> GetBlogsByCategoryAsync(string category, int page = 1, int pageSize = 10);
        Task<IEnumerable<Blog>> GetFeaturedBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Blog>> GetPremiumBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Blog>> SearchBlogsAsync(BlogSearchDto searchDto);
        Task<IEnumerable<Blog>> GetPopularBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Blog>> GetRecentBlogsAsync(int page = 1, int pageSize = 10);
        Task<bool> CreateBlogAsync(Blog blog);
        Task<bool> UpdateBlogAsync(Blog blog);
        Task<bool> DeleteBlogAsync(Guid blogId);
        Task<bool> BlogExistsAsync(Guid blogId);
        Task<bool> IncrementViewCountAsync(Guid blogId);
        Task<bool> UpdateLikeCountAsync(Guid blogId);

        // Blog Comment Operations
        Task<BlogComment?> GetCommentByIdAsync(Guid commentId);
        Task<IEnumerable<BlogComment>> GetCommentsByBlogIdAsync(Guid blogId);
        Task<IEnumerable<BlogComment>> GetCommentsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<bool> CreateCommentAsync(BlogComment comment);
        Task<bool> UpdateCommentAsync(BlogComment comment);
        Task<bool> DeleteCommentAsync(Guid commentId);
        Task<bool> ApproveCommentAsync(Guid commentId);
        Task<bool> RejectCommentAsync(Guid commentId);

        // Blog Like Operations
        Task<BlogLike?> GetBlogLikeAsync(Guid blogId, Guid userId);
        Task<IEnumerable<BlogLike>> GetBlogLikesAsync(Guid blogId);
        Task<bool> CreateBlogLikeAsync(BlogLike blogLike);
        Task<bool> DeleteBlogLikeAsync(Guid blogId, Guid userId);
        Task<bool> UserHasLikedBlogAsync(Guid blogId, Guid userId);

        // Tag Operations
        Task<Tag?> GetTagByIdAsync(Guid tagId);
        Task<Tag?> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 10);
        Task<IEnumerable<Tag>> GetTagsByBlogIdAsync(Guid blogId);
        Task<bool> CreateTagAsync(Tag tag);
        Task<bool> UpdateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(Guid tagId);
        Task<bool> TagExistsAsync(string name);
        Task<bool> IncrementTagUsageAsync(Guid tagId);
        Task<bool> DecrementTagUsageAsync(Guid tagId);

        // Blog-Tag Association Operations
        Task<bool> AddBlogTagAsync(Guid blogId, Guid tagId);
        Task<bool> RemoveBlogTagAsync(Guid blogId, Guid tagId);
        Task<bool> RemoveAllBlogTagsAsync(Guid blogId);

        // Statistics and Counts
        Task<int> GetTotalBlogsCountAsync();
        Task<int> GetTotalCountByMonthAsync(int year, int month);
        Task<int> GetPublishedBlogsCountAsync();
        Task<int> GetTotalPublishedCountByMonthAsync(int year, int month);
        Task<int> GetBlogsByUserCountAsync(Guid userId);
        Task<int> GetBlogsByCategoryCountAsync(string category);
        Task<int> GetFeaturedBlogsCountAsync();
        Task<int> GetTotalFeaturedCountByMonthAsync(int year, int month);
        Task<int> GetPremiumBlogsCountAsync();
        Task<int> GetTotalPremiumCountByMonthAsync(int year, int month);
        Task<int> GetTotalCommentsCountAsync();
        Task<int> GetCommentsByBlogCountAsync(Guid blogId);
        Task<int> GetTotalLikesCountAsync();
        Task<int> GetLikesByBlogCountAsync(Guid blogId);
        Task<int> GetTotalViewsCountAsync();
        Task<int> GetTotalViewsCountByMonthAsync(int year, int month);
        Task<IEnumerable<string>> GetDistinctCategoriesAsync();
    }
}

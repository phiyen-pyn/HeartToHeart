using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IBlogService
    {
        // Blog CRUD Operations
        Task<BlogDto?> GetBlogByIdAsync(Guid blogId, Guid? currentUserId = null);
        Task<IEnumerable<BlogDto>> GetAllBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogDto>> GetPublishedBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogDto>> GetBlogsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogDto>> GetBlogsByCategoryAsync(string category, int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogDto>> GetFeaturedBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogDto>> GetPremiumBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogDto>> SearchBlogsAsync(BlogSearchDto searchDto);
        Task<IEnumerable<BlogDto>> GetPopularBlogsAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<BlogDto>> GetRecentBlogsAsync(int page = 1, int pageSize = 10);
        Task<BlogDto?> CreateBlogAsync(Guid userId, CreateBlogDto createDto);
        Task<bool> UpdateBlogAsync(Guid blogId, UpdateBlogDto updateDto, Guid userId);
        Task<bool> PublishBlogAsync(Guid blogId, Guid userId);
        Task<bool> UnpublishBlogAsync(Guid blogId, Guid userId);
        Task<bool> DeleteBlogAsync(Guid blogId, Guid userId);
        Task<bool> IncrementViewCountAsync(Guid blogId);

        // Blog Comment Operations
        Task<BlogCommentDto?> GetCommentByIdAsync(Guid commentId);
        Task<IEnumerable<BlogCommentDto>> GetCommentsByBlogIdAsync(Guid blogId);
        Task<IEnumerable<BlogCommentDto>> GetCommentsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<BlogCommentDto?> CreateCommentAsync(Guid userId, CreateBlogCommentDto createDto);
        Task<bool> UpdateCommentAsync(Guid commentId, UpdateBlogCommentDto updateDto, Guid userId);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
        Task<bool> ApproveCommentAsync(Guid commentId);
        Task<bool> RejectCommentAsync(Guid commentId);

        // Blog Like Operations
        Task<bool> LikeBlogAsync(Guid blogId, Guid userId);
        Task<bool> UnlikeBlogAsync(Guid blogId, Guid userId);
        Task<bool> UserHasLikedBlogAsync(Guid blogId, Guid userId);
        Task<IEnumerable<BlogLikeDto>> GetBlogLikesAsync(Guid blogId);

        // Tag Operations
        Task<TagDto?> GetTagByIdAsync(Guid tagId);
        Task<TagDto?> GetTagByNameAsync(string name);
        Task<IEnumerable<TagDto>> GetAllTagsAsync();
        Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10);
        Task<IEnumerable<TagDto>> GetTagsByBlogIdAsync(Guid blogId);
        Task<TagDto?> CreateTagAsync(CreateTagDto createDto);
        Task<bool> UpdateTagAsync(Guid tagId, UpdateTagDto updateDto);
        Task<bool> DeleteTagAsync(Guid tagId);

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
        Task<BlogStatsDto> GetBlogStatisticsAsync();
    }
}

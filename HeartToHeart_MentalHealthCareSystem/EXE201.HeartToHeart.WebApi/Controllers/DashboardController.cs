using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IAnonymousPostService _postService;
        private readonly IAppointmentService _appointmentService;
        private readonly IBlogService _blogService;
        private readonly ICounselorService _counselorService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IUserService _userService;

        public DashboardController(
            IAnonymousPostService postService,
            IAppointmentService appointmentService,
            IBlogService blogService,
            ICounselorService counselorService,
            IPaymentTransactionService paymentTransactionService,
            IUserService userService)
        {
            _postService = postService;
            _appointmentService = appointmentService;
            _blogService = blogService;
            _counselorService = counselorService;
            _paymentTransactionService = paymentTransactionService;
            _userService = userService;
        }

        [HttpGet("post-stats")]
        public async Task<IActionResult> GetPostStatsAsync()
        {
            var totalCount = await _postService.GetTotalCountAsync();
            var totalReportedCount = await _postService.GetTotalReportedCountAsync();
            var totalThisMonth = await _postService.GetTotalCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalReportedThisMonth = await _postService.GetTotalReportedCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            return Ok(new
            {
                TotalPostCount = totalCount,
                TotalPostReportedCount = totalReportedCount,
                TotalPostThisMonth = totalThisMonth,
                TotalPostReportedThisMonth = totalReportedThisMonth
            });
        }

        [HttpGet("post-stats-by-year/{year}")]
        public async Task<IActionResult> GetPostStatsByYearAsync(int year)
        {
            var results = new List<object>();
            for (int month = 1; month <= 12; month++)
            {
                var total = await _postService.GetTotalCountByMonthAsync(year, month);
                var reported = await _postService.GetTotalReportedCountByMonthAsync(year, month);
                results.Add(new
                {
                    Month = $"{month:D2}/{year}",
                    TotalPostThisMonth = total,
                    TotalPostReportedThisMonth = reported
                });
            }
            return Ok(results);
        }

        [HttpGet("appointment-stats")]
        public async Task<IActionResult> GetAppointmentStatsAsync()
        {
            var totalCount = await _appointmentService.GetTotalAppointmentsCountAsync();
            var totalThisMonth = await _appointmentService.GetTotalCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            return Ok(new
            {
                TotalAppointmentCount = totalCount,
                TotalAppointmentThisMonth = totalThisMonth
            });
        }

        [HttpGet("appointment-stats-by-year/{year}")]
        public async Task<IActionResult> GetAppointmentStatsByYearAsync(int year)
        {
            var results = new List<object>();
            for (int month = 1; month <= 12; month++)
            {
                var total = await _appointmentService.GetTotalCountByMonthAsync(year, month);
                results.Add(new
                {
                    Month = $"{month:D2}/{year}",
                    TotalAppointmentThisMonth = total
                });
            }
            return Ok(results);
        }

        [HttpGet("blog-stats")]
        public async Task<IActionResult> GetBlogStatsAsync()
        {
            var totalCount = await _blogService.GetTotalBlogsCountAsync();
            var totalThisMonth = await _blogService.GetTotalCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalPublishedCount = await _blogService.GetPublishedBlogsCountAsync();
            var totalPublishedThisMonth = await _blogService.GetTotalPublishedCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalFeaturedCount = await _blogService.GetFeaturedBlogsCountAsync();
            var totalFeaturedThisMonth = await _blogService.GetTotalFeaturedCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalPremiumCount = await _blogService.GetPremiumBlogsCountAsync();
            var totalPremiumThisMonth = await _blogService.GetTotalPremiumCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalViewCount = await _blogService.GetTotalViewsCountAsync();
            var totalViewThisMonth = await _blogService.GetTotalViewsCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            return Ok(new
            {
                TotalBlogCount = totalCount,
                TotalBlogThisMonth = totalThisMonth,
                TotalPublishedBlogCount = totalPublishedCount,
                TotalPublishedBlogThisMonth = totalPublishedThisMonth,
                TotalFeaturedBlogCount = totalFeaturedCount,
                TotalFeaturedBlogThisMonth = totalFeaturedThisMonth,
                TotalPremiumBlogCount = totalPremiumCount,
                TotalPremiumBlogThisMonth = totalPremiumThisMonth,
                TotalViewCount = totalViewCount,
                TotalViewThisMonth = totalViewThisMonth
            });
        }

        [HttpGet("blog-stats-by-year/{year}")]
        public async Task<IActionResult> GetBlogStatsByYearAsync(int year)
        {
            var results = new List<object>();
            for (int month = 1; month <= 12; month++)
            {
                var total = await _blogService.GetTotalCountByMonthAsync(year, month);
                var published = await _blogService.GetTotalPublishedCountByMonthAsync(year, month);
                var featured = await _blogService.GetTotalFeaturedCountByMonthAsync(year, month);
                var premium = await _blogService.GetTotalPremiumCountByMonthAsync(year, month);
                var views = await _blogService.GetTotalViewsCountByMonthAsync(year, month);
                results.Add(new
                {
                    Month = $"{month:D2}/{year}",
                    TotalBlogThisMonth = total,
                    TotalPublishedBlogThisMonth = published,
                    TotalFeaturedBlogThisMonth = featured,
                    TotalPremiumBlogThisMonth = premium,
                    TotalViewThisMonth = views
                });
            }
            return Ok(results);
        }

        [HttpGet("payment-stats")]
        public async Task<IActionResult> GetPaymentStatsAsync()
        {
            var totalCount = await _paymentTransactionService.GetTotalCountAsync();
            var totalPendingCount = await _paymentTransactionService.GetTotalCountByStatusAsync(PaymentStatus.Pending);
            var totalCompletedCount = await _paymentTransactionService.GetTotalCountByStatusAsync(PaymentStatus.Completed);
            var totalFailedCount = await _paymentTransactionService.GetTotalCountByStatusAsync(PaymentStatus.Failed);
            var totalRefundedCount = await _paymentTransactionService.GetTotalCountByStatusAsync(PaymentStatus.Refunded);
            var totalThisMonth = await _paymentTransactionService.GetTotalCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalPendingThisMonth = await _paymentTransactionService.GetTotalCountByStatusAndMonthAsync(PaymentStatus.Pending, DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalCompletedThisMonth = await _paymentTransactionService.GetTotalCountByStatusAndMonthAsync(PaymentStatus.Completed, DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalFailedThisMonth = await _paymentTransactionService.GetTotalCountByStatusAndMonthAsync(PaymentStatus.Failed, DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalRefundedThisMonth = await _paymentTransactionService.GetTotalCountByStatusAndMonthAsync(PaymentStatus.Refunded, DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalAmount = await _paymentTransactionService.GetTotalAmountAsync();
            var totalAmountThisMonth = await _paymentTransactionService.GetTotalAmountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            return Ok(new
            {
                TotalPaymentTransactionCount = totalCount,
                TotalPendingCount = totalPendingCount,
                TotalCompletedCount = totalCompletedCount,
                TotalFailedCount = totalFailedCount,
                TotalRefundedCount = totalRefundedCount,
                TotalPaymentTransactionThisMonth = totalThisMonth,
                TotalPendingThisMonth = totalPendingThisMonth,
                TotalCompletedThisMonth = totalCompletedThisMonth,
                TotalFailedThisMonth = totalFailedThisMonth,
                TotalRefundedThisMonth = totalRefundedThisMonth,
                TotalAmount = totalAmount,
                TotalAmountThisMonth = totalAmountThisMonth
            });
        }

        [HttpGet("payment-stats-by-year/{year}")]
        public async Task<IActionResult> GetPaymentStatsByYearAsync(int year)
        {
            var results = new List<object>();
            for (int month = 1; month <= 12; month++)
            {
                var total = await _paymentTransactionService.GetTotalCountByMonthAsync(year, month);
                var pending = await _paymentTransactionService.GetTotalCountByStatusAndMonthAsync(PaymentStatus.Pending, year, month);
                var completed = await _paymentTransactionService.GetTotalCountByStatusAndMonthAsync(PaymentStatus.Completed, year, month);
                var failed = await _paymentTransactionService.GetTotalCountByStatusAndMonthAsync(PaymentStatus.Failed, year, month);
                var refunded = await _paymentTransactionService.GetTotalCountByStatusAndMonthAsync(PaymentStatus.Refunded, year, month);
                var amount = await _paymentTransactionService.GetTotalAmountByMonthAsync(year, month);
                results.Add(new
                {
                    Month = $"{month:D2}/{year}",
                    TotalPaymentTransactionThisMonth = total,
                    TotalPendingThisMonth = pending,
                    TotalCompletedThisMonth = completed,
                    TotalFailedThisMonth = failed,
                    TotalRefundedThisMonth = refunded,
                    TotalAmountThisMonth = amount
                });
            }
            return Ok(results);
        }

        [HttpGet("user-stats")]
        public async Task<IActionResult> GetUserStatsAsync()
        {
            var totalCount = await _userService.GetTotalUsersCountAsync();
            var totalMembersCount = await _userService.GetTotalUsersCountByRoleAsync(Roles.Member);
            var totalPremiumMembersCount = await _userService.GetTotalUsersCountByRoleAsync(Roles.Premium);
            var totalCounselorsCount = await _counselorService.GetTotalCounselorsCountAsync();
            var totalVerifiedCounselorsCount = await _counselorService.GetVerifiedCounselorsCountAsync();
            var totalAdminsCount = await _userService.GetTotalUsersCountByRoleAsync(Roles.Admin);
            var totalStaffCount = await _userService.GetTotalUsersCountByRoleAsync(Roles.Staff);
            var totalCountThisMonth = await _userService.GetTotalCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalMembersThisMonth = await _userService.GetTotalCountByRoleAndMonthAsync(Roles.Member, DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalPremiumMembersThisMonth = await _userService.GetTotalCountByRoleAndMonthAsync(Roles.Premium, DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalCounselorsThisMonth = await _counselorService.GetTotalCounselorsCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalVerifiedCounselorsThisMonth = await _counselorService.GetVerifiedCounselorsCountByMonthAsync(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalAdminsThisMonth = await _userService.GetTotalCountByRoleAndMonthAsync(Roles.Admin, DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var totalStaffThisMonth = await _userService.GetTotalCountByRoleAndMonthAsync(Roles.Staff, DateTime.UtcNow.Year, DateTime.UtcNow.Month);

            return Ok(new
            {
                TotalUserCount = totalCount,
                TotalMembersCount = totalMembersCount,
                TotalPremiumMembersCount = totalPremiumMembersCount,
                TotalCounselorsCount = totalCounselorsCount,
                TotalVerifiedCounselorsCount = totalVerifiedCounselorsCount,
                TotalAdminsCount = totalAdminsCount,
                TotalStaffCount = totalStaffCount,
                TotalUserThisMonth = totalCountThisMonth,
                TotalMembersThisMonth = totalMembersThisMonth,
                TotalPremiumMembersThisMonth = totalPremiumMembersThisMonth,
                TotalCounselorsThisMonth = totalCounselorsThisMonth,
                TotalVerifiedCounselorsThisMonth = totalVerifiedCounselorsThisMonth,
                TotalAdminsThisMonth = totalAdminsThisMonth,
                TotalStaffThisMonth = totalStaffThisMonth
            });
        }

        [HttpGet("user-stats-by-year/{year}")]
        public async Task<IActionResult> GetUserStatsByYearAsync(int year)
        {
            var results = new List<object>();
            for (int month = 1; month <= 12; month++)
            {
                var total = await _userService.GetTotalCountByMonthAsync(year, month);
                var members = await _userService.GetTotalCountByRoleAndMonthAsync(Roles.Member, year, month);
                var premiumMembers = await _userService.GetTotalCountByRoleAndMonthAsync(Roles.Premium, year, month);
                var counselors = await _counselorService.GetTotalCounselorsCountByMonthAsync(year, month);
                var verifiedCounselors = await _counselorService.GetVerifiedCounselorsCountByMonthAsync(year, month);
                var admins = await _userService.GetTotalCountByRoleAndMonthAsync(Roles.Admin, year, month);
                var staff = await _userService.GetTotalCountByRoleAndMonthAsync(Roles.Staff, year, month);
                results.Add(new
                {
                    Month = $"{month:D2}/{year}",
                    TotalUserThisMonth = total,
                    TotalMembersThisMonth = members,
                    TotalPremiumMembersThisMonth = premiumMembers,
                    TotalCounselorsThisMonth = counselors,
                    TotalVerifiedCounselorsThisMonth = verifiedCounselors,
                    TotalAdminsThisMonth = admins,
                    TotalStaffThisMonth = staff
                });
            }
            return Ok(results);
        }
    }
}

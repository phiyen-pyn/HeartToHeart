using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IAnonymousPostRepository _anonymousPostRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IReportRepository reportRepository, IAnonymousPostRepository anonymousPostRepository, ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _anonymousPostRepository = anonymousPostRepository;
            _logger = logger;
        }
        public async Task<bool> CreateAsync(ReportDto dto)
        {
            try
            {
                var report = new Report
                {
                    PostId = dto.PostId,
                    ReporterId = dto.ReporterId,
                    Reason = dto.Reason,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                };

                var result = await _reportRepository.CreateAsync(report);
                var post = await _anonymousPostRepository.GetByIdAsync(report.PostId);
                if (post != null)
                {
                    post.IsReported = true;
                    post.UpdatedAt = DateTime.UtcNow;
                    await _anonymousPostRepository.UpdateAsync(post);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid reportId, ReportDto dto)
        {
            try
            {
                var existing = await _reportRepository.GetByIdAsync(reportId);
                if (existing == null) return false;

                existing.Reason = dto.Reason;
                existing.Status = dto.Status;
                existing.UpdatedAt = DateTime.UtcNow;
                var result = await _reportRepository.UpdateAsync(existing);

                var post = await _anonymousPostRepository.GetByIdAsync(existing.PostId);
                if (post != null)
                {
                    var hasPendingReports = post.Reports
                        .Any(r => r.IsActive == true && r.Status == "Pending");

                    if (!hasPendingReports)
                    {
                        post.IsReported = false;
                        post.UpdatedAt = DateTime.UtcNow;
                        await _anonymousPostRepository.UpdateAsync(post);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", reportId);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid reportId)
        {
            try
            {
                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                {
                    _logger.LogWarning("Report not found for deactivation: {ReportId}", reportId);
                    return false;
                }

                report.IsActive = false;
                report.UpdatedAt = DateTime.UtcNow;
                var result = await _reportRepository.UpdateAsync(report);

                var post = await _anonymousPostRepository.GetByIdAsync(report.PostId);
                if (post != null)
                {
                    var hasPendingReports = post.Reports
                        .Any(r => r.IsActive == true && r.Status == "Pending");

                    if (!hasPendingReports)
                    {
                        post.IsReported = false;
                        post.UpdatedAt = DateTime.UtcNow;
                        await _anonymousPostRepository.UpdateAsync(post);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating report: {ReportId}", reportId);
                return false;
            }
        }

        public async Task<IEnumerable<ReportDto>> GetAllByPostIdAsync(Guid postId)
        {
            try
            {
                var reports = await _reportRepository.GetAllByPostIdAsync(postId);
                return reports.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reports by postId.");
                return Enumerable.Empty<ReportDto>();
            }
        }

        public async Task<ReportDto?> GetByIdAsync(Guid reportId)
        {
            try
            {
                var report = await _reportRepository.GetByIdAsync(reportId);
                return report == null ? null : MapToDto(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report by ID: {ReportId}", reportId);
                return null;
            }
        }

        public static ReportDto MapToDto(Report report)
        {
            return new ReportDto
            {
                Id = report.Id,
                PostId = report.PostId,
                ReporterId = report.ReporterId,
                Reason = report.Reason,
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
            };
        }
    }
}

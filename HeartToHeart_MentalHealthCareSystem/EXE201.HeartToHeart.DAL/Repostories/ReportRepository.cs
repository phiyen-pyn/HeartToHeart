using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Report>> GetAllByPostIdAsync(Guid postId)
        {
            return await _context.Reports
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Report?> GetByIdAsync(Guid reportId)
        {
            return await _context.Reports
                .Include(c => c.Reporter)
                .FirstOrDefaultAsync(c => c.Id == reportId);
        }

        public async Task<bool> CreateAsync(Report report)
        {
            try
            {
                _context.Reports.Add(report);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Report report)
        {
            try
            {
                _context.Reports.Update(report);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid reportId)
        {
            var report = await GetByIdAsync(reportId);
            if (report == null) return false;
            try
            {
                _context.Reports.Remove(report);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

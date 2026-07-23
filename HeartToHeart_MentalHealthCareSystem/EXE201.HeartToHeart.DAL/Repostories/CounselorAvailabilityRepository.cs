using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class CounselorAvailabilityRepository : ICounselorAvailabilityRepository
    {
        private readonly ApplicationDbContext _context;

        public CounselorAvailabilityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CounselorAvailability?> GetCounselorAvailabilityForDateAsync(Guid counselorId, DateTime date)
        {
            var targetDate = date.Date;
            return await _context.CounselorAvailabilities
                .FirstOrDefaultAsync(ca => ca.CounselorId == counselorId &&
                                         ca.Date.Date == targetDate);
        }

        public async Task<IEnumerable<CounselorAvailability>> GetCounselorAvailabilityRangeAsync(Guid counselorId, DateTime fromDate, DateTime toDate)
        {
            return await _context.CounselorAvailabilities
                .Where(ca => ca.CounselorId == counselorId &&
                           ca.Date >= fromDate.Date &&
                           ca.Date <= toDate.Date)
                .OrderBy(ca => ca.Date)
                .ToListAsync();
        }

        public async Task<bool> SetCounselorAvailabilityAsync(CounselorAvailability availability)
        {
            try
            {
                // Check if availability already exists for this date
                var existing = await GetCounselorAvailabilityForDateAsync(availability.CounselorId, availability.Date);
                if (existing != null)
                {
                    // Update existing
                    existing.IsAvailable = availability.IsAvailable;
                    existing.StartTime = availability.StartTime;
                    existing.EndTime = availability.EndTime;
                    existing.Notes = availability.Notes;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.CounselorAvailabilities.Update(existing);
                }
                else
                {
                    // Add new
                    _context.CounselorAvailabilities.Add(availability);
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCounselorAvailabilityAsync(CounselorAvailability availability)
        {
            try
            {
                _context.CounselorAvailabilities.Update(availability);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCounselorAvailabilityAsync(Guid availabilityId)
        {
            try
            {
                var availability = await _context.CounselorAvailabilities.FindAsync(availabilityId);
                if (availability == null) return false;

                _context.CounselorAvailabilities.Remove(availability);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<CounselorScheduleTemplate>> GetCounselorScheduleTemplateAsync(Guid counselorId)
        {
            return await _context.CounselorScheduleTemplates
                .Where(cst => cst.CounselorId == counselorId)
                .OrderBy(cst => cst.DayOfWeek)
                .ToListAsync();
        }

        public async Task<bool> SetCounselorScheduleTemplateAsync(CounselorScheduleTemplate template)
        {
            try
            {
                // Check if template already exists for this day
                var existing = await _context.CounselorScheduleTemplates
                    .FirstOrDefaultAsync(cst => cst.CounselorId == template.CounselorId &&
                                              cst.DayOfWeek == template.DayOfWeek);

                if (existing != null)
                {
                    // Update existing
                    existing.IsAvailable = template.IsAvailable;
                    existing.StartTime = template.StartTime;
                    existing.EndTime = template.EndTime;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.CounselorScheduleTemplates.Update(existing);
                }
                else
                {
                    // Add new
                    _context.CounselorScheduleTemplates.Add(template);
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCounselorScheduleTemplateAsync(CounselorScheduleTemplate template)
        {
            try
            {
                _context.CounselorScheduleTemplates.Update(template);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCounselorScheduleTemplateAsync(Guid templateId)
        {
            try
            {
                var template = await _context.CounselorScheduleTemplates.FindAsync(templateId);
                if (template == null) return false;

                _context.CounselorScheduleTemplates.Remove(template);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

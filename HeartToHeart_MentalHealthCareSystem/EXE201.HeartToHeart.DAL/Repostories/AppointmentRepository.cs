using EXE201.HeartToHeart.Common.Constants;
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
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Counselor)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.Appointments
                .Include(a => a.User) 
                .Include(a => a.Counselor)
                .ThenInclude(c => c.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCounselorIdAsync(Guid counselorId, int page = 1, int pageSize = 10)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.CounselorId == counselorId)
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(string status, int page = 1, int pageSize = 10)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Counselor)
                .ThenInclude(c => c.User)
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Counselor)
                .ThenInclude(c => c.User)
                .Where(a => a.AppointmentDate >= fromDate && a.AppointmentDate <= toDate)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> SearchAppointmentsAsync(AppointmentSearchDto searchDto)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Counselor)
                .ThenInclude(c => c.User)
                .AsQueryable();

            if (searchDto.UserId.HasValue)
                query = query.Where(a => a.UserId == searchDto.UserId.Value);

            if (searchDto.CounselorId.HasValue)
                query = query.Where(a => a.CounselorId == searchDto.CounselorId.Value);

            if (!string.IsNullOrEmpty(searchDto.Status))
                query = query.Where(a => a.Status == searchDto.Status);

            if (searchDto.FromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= searchDto.FromDate.Value);

            if (searchDto.ToDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= searchDto.ToDate.Value);

            return await query
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetCounselorAppointmentsForDateAsync(Guid counselorId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Appointments
                .Where(a => a.CounselorId == counselorId &&
                           a.AppointmentDate >= startOfDay &&
                           a.AppointmentDate < endOfDay &&
                           (a.Status == AppointmentConstants.Status.Confirmed ||
                            a.Status == AppointmentConstants.Status.Pending ||
                            a.Status == AppointmentConstants.Status.Rescheduled))
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetCounselorAppointmentsForDateRangeAsync(Guid counselorId, DateTime fromDate, DateTime toDate)
        {
            return await _context.Appointments
                .Where(a => a.CounselorId == counselorId &&
                           a.AppointmentDate >= fromDate &&
                           a.AppointmentDate <= toDate &&
                           (a.Status == AppointmentConstants.Status.Confirmed ||
                            a.Status == AppointmentConstants.Status.Pending ||
                            a.Status == AppointmentConstants.Status.Rescheduled))
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<bool> HasConflictingAppointmentAsync(Guid counselorId, DateTime appointmentDate, int durationMinutes, Guid? excludeAppointmentId = null)
        {
            var appointmentEnd = appointmentDate.AddMinutes(durationMinutes);

            var query = _context.Appointments
                .Where(a => a.CounselorId == counselorId &&
                           (a.Status == AppointmentConstants.Status.Confirmed ||
                            a.Status == AppointmentConstants.Status.Pending ||
                            a.Status == AppointmentConstants.Status.Rescheduled));

            if (excludeAppointmentId.HasValue)
                query = query.Where(a => a.Id != excludeAppointmentId.Value);

            return await query.AnyAsync(a =>
                (appointmentDate < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
                 appointmentEnd > a.AppointmentDate));
        }

        public async Task<bool> CreateAppointmentAsync(Appointment appointment)
        {
            try
            {
                if (appointment.Id == Guid.Empty)
                    appointment.Id = Guid.NewGuid();

                if (appointment.CreatedAt == DateTime.MinValue)
                    appointment.CreatedAt = DateTime.UtcNow;

                appointment.UpdatedAt = DateTime.UtcNow;

                _context.Appointments.Add(appointment);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating appointment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAppointmentAsync(Appointment appointment)
        {
            try
            {
                appointment.UpdatedAt = DateTime.UtcNow;
                _context.Appointments.Update(appointment);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating appointment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAppointmentAsync(Guid appointmentId)
        {
            try
            {
                var appointment = await GetAppointmentByIdAsync(appointmentId);
                if (appointment == null) return false;

                _context.Appointments.Remove(appointment);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting appointment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AppointmentExistsAsync(Guid appointmentId)
        {
            return await _context.Appointments.AnyAsync(a => a.Id == appointmentId);
        }

        public async Task<int> GetTotalAppointmentsCountAsync()
        {
            return await _context.Appointments.CountAsync();
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            return await _context.Appointments
                .CountAsync(a => a.AppointmentDate.Year == year && a.AppointmentDate.Month == month);
        }

        public async Task<int> GetTotalAppointmentsCountByUserAsync(Guid userId)
        {
            return await _context.Appointments.CountAsync(a => a.UserId == userId);
        }

        public async Task<int> GetTotalAppointmentsCountByCounselorAsync(Guid counselorId)
        {
            return await _context.Appointments.CountAsync(a => a.CounselorId == counselorId);
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(Guid userId, int days = 7)
        {
            var fromDate = DateTime.UtcNow;
            var toDate = fromDate.AddDays(days);

            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Counselor)
                .ThenInclude(c => c.User)
                .Where(a => a.UserId == userId &&
                           a.AppointmentDate >= fromDate &&
                           a.AppointmentDate <= toDate &&
                           (a.Status == AppointmentConstants.Status.Confirmed ||
                            a.Status == AppointmentConstants.Status.Pending))
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentHistoryAsync(Guid userId, int months = 3)
        {
            var fromDate = DateTime.UtcNow.AddMonths(-months);

            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Counselor)
                .ThenInclude(c => c.User)
                .Where(a => a.UserId == userId &&
                           a.AppointmentDate >= fromDate &&
                           a.Status == AppointmentConstants.Status.Completed)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetCounselorTodayAppointmentsAsync(Guid counselorId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.CounselorId == counselorId &&
                           a.AppointmentDate >= today &&
                           a.AppointmentDate < tomorrow &&
                           (a.Status == AppointmentConstants.Status.Confirmed ||
                            a.Status == AppointmentConstants.Status.Pending))
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<bool> UserHasAppointmentWithCounselorAsync(Guid userId, Guid counselorId)
        {
            return await _context.Appointments
                .AnyAsync(a => a.UserId == userId &&
                              a.CounselorId == counselorId &&
                              a.Status == AppointmentConstants.Status.Completed);
        }

        public async Task<int> GetCounselorAppointmentCountForDateAsync(Guid counselorId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Appointments
                .CountAsync(a => a.CounselorId == counselorId &&
                               a.AppointmentDate >= startOfDay &&
                               a.AppointmentDate < endOfDay &&
                               (a.Status == AppointmentConstants.Status.Confirmed ||
                                a.Status == AppointmentConstants.Status.Pending));
        }

        public async Task<Dictionary<string, int>> GetAppointmentStatisticsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= toDate.Value);

            return await query
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<bool> CreateAppointmentHistoryAsync(Guid appointmentId, string action, string oldStatus, string newStatus, Guid changedByUserId, string? notes = null)
        {
            try
            {
                var history = new AppointmentHistory
                {
                    AppointmentId = appointmentId,
                    Action = action,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedByUserId = changedByUserId,
                    Notes = notes
                };

                _context.AppointmentHistories.Add(history);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<AppointmentHistory>> GetAppointmentHistoryAsync(Guid appointmentId)
        {
            return await _context.AppointmentHistories
                .Include(ah => ah.ChangedByUser)
                .Where(ah => ah.AppointmentId == appointmentId)
                .OrderBy(ah => ah.CreatedAt)
                .ToListAsync();
        }
    }
}

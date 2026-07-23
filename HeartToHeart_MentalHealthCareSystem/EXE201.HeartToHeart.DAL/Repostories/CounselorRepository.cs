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
    public class CounselorRepository : ICounselorRepository
    {
        private readonly ApplicationDbContext _context;

        public CounselorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Counselor?> GetCounselorByIdAsync(Guid counselorId)
        {
            return await _context.Counselors
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == counselorId);
        }

        public async Task<Counselor?> GetCounselorByUserIdAsync(Guid userId)
        {
            return await _context.Counselors
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<IEnumerable<Counselor>> GetAvailableCounselorsAsync()
        {
            return await _context.Counselors
                .Include(c => c.User)
                .Where(c => c.IsAvailable && c.IsVerified && c.User.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Counselor>> GetCounselorsBySpecializationAsync(string specialization)
        {
            return await _context.Counselors
                .Include(c => c.User)
                .Where(c => c.Specialization == specialization && c.IsAvailable && c.IsVerified && c.User.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Counselor>> GetAllCounselorsAsync()
        {
            return await _context.Counselors
                .Include(c => c.User)
                .Where(c => c.User.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsCounselorAvailableAsync(Guid counselorId)
        {
            var counselor = await _context.Counselors
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == counselorId);

            return counselor != null && counselor.IsAvailable && counselor.IsVerified && counselor.User.IsActive;
        }

        public async Task<bool> CreateCounselorAsync(Counselor counselor)
        {
            try
            {
                if (counselor.Id == Guid.Empty)
                    counselor.Id = Guid.NewGuid();

                if (counselor.CreatedAt == DateTime.MinValue)
                    counselor.CreatedAt = DateTime.UtcNow;

                counselor.UpdatedAt = DateTime.UtcNow;

                _context.Counselors.Add(counselor);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating counselor: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCounselorAsync(Counselor counselor)
        {
            try
            {
                counselor.UpdatedAt = DateTime.UtcNow;
                _context.Counselors.Update(counselor);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating counselor: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCounselorAsync(Guid counselorId)
        {
            try
            {
                var counselor = await GetCounselorByIdAsync(counselorId);
                if (counselor == null) return false;

                _context.Counselors.Remove(counselor);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting counselor: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CounselorExistsAsync(Guid counselorId)
        {
            return await _context.Counselors.AnyAsync(c => c.Id == counselorId);
        }

        public async Task<bool> UserIsCounselorAsync(Guid userId)
        {
            return await _context.Counselors.AnyAsync(c => c.UserId == userId);
        }

        public async Task<int> GetTotalCounselorsCountAsync()
        {
            return await _context.Counselors.CountAsync();
        }
        public async Task<int> GetTotalCounselorsCountByMonthAsync(int year, int month)
        {
            return await _context.Counselors
                .CountAsync(c => c.CreatedAt.Year == year && c.CreatedAt.Month == month);
        }

        public async Task<int> GetVerifiedCounselorsCountAsync()
        {
            return await _context.Counselors.CountAsync(c => c.IsVerified);
        }

        public async Task<int> GetVerifiedCounselorsCountByMonthAsync(int year, int month)
        {
            return await _context.Counselors
                .CountAsync(c => c.IsVerified && c.CreatedAt.Year == year && c.CreatedAt.Month == month);
        }

        public async Task<int> GetAvailableCounselorsCountAsync()
        {
            return await _context.Counselors.CountAsync(c => c.IsAvailable && c.IsVerified && c.User.IsActive);
        }

        public async Task<IEnumerable<Appointment>> GetCounselorAppointmentsAsync(Guid counselorId, int page = 1, int pageSize = 10)
        {
            return await _context.Appointments
                .Include(a => a.User)  
                .Include(a => a.Counselor)
                    .ThenInclude(c => c.User)  
                .Where(a => a.CounselorId == counselorId)
                .OrderByDescending(a => a.AppointmentDate)  
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
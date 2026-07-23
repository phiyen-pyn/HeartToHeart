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
    public class HolidayRepository : IHolidayRepository
    {
        private readonly ApplicationDbContext _context;

        public HolidayRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsHolidayAsync(DateTime date, string countryCode = "VN")
        {
            var targetDate = date.Date;
            return await _context.Holidays
                .AnyAsync(h => h.Date.Date == targetDate &&
                              h.CountryCode == countryCode &&
                              h.IsActive);
        }

        public async Task<Holiday?> GetHolidayInfoAsync(DateTime date, string countryCode = "VN")
        {
            var targetDate = date.Date;
            return await _context.Holidays
                .FirstOrDefaultAsync(h => h.Date.Date == targetDate &&
                                        h.CountryCode == countryCode &&
                                        h.IsActive);
        }

        public async Task<IEnumerable<Holiday>> GetHolidaysForYearAsync(int year, string countryCode = "VN")
        {
            return await _context.Holidays
                .Where(h => h.Date.Year == year &&
                           h.CountryCode == countryCode &&
                           h.IsActive)
                .OrderBy(h => h.Date)
                .ToListAsync();
        }

        public async Task<bool> AddHolidayAsync(Holiday holiday)
        {
            try
            {
                _context.Holidays.Add(holiday);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateHolidayAsync(Holiday holiday)
        {
            try
            {
                _context.Holidays.Update(holiday);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteHolidayAsync(Guid holidayId)
        {
            try
            {
                var holiday = await _context.Holidays.FindAsync(holidayId);
                if (holiday == null) return false;

                _context.Holidays.Remove(holiday);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Holiday>> GetUpcomingHolidaysAsync(string countryCode = "VN", int days = 30)
        {
            var fromDate = DateTime.Today;
            var toDate = fromDate.AddDays(days);

            return await _context.Holidays
                .Where(h => h.Date >= fromDate &&
                           h.Date <= toDate &&
                           h.CountryCode == countryCode &&
                           h.IsActive)
                .OrderBy(h => h.Date)
                .ToListAsync();
        }
    }
}

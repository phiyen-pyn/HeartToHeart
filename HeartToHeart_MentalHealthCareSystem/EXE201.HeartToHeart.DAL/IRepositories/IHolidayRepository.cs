using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IHolidayRepository
    {
        Task<bool> IsHolidayAsync(DateTime date, string countryCode = "VN");
        Task<Holiday?> GetHolidayInfoAsync(DateTime date, string countryCode = "VN");
        Task<IEnumerable<Holiday>> GetHolidaysForYearAsync(int year, string countryCode = "VN");
        Task<bool> AddHolidayAsync(Holiday holiday);
        Task<bool> UpdateHolidayAsync(Holiday holiday);
        Task<bool> DeleteHolidayAsync(Guid holidayId);
        Task<IEnumerable<Holiday>> GetUpcomingHolidaysAsync(string countryCode = "VN", int days = 30);
    }
}

using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class HolidaySeederService : IHolidaySeederService
    {
        private readonly IHolidayRepository _holidayRepository;
        private readonly ILogger<HolidaySeederService> _logger;

        public HolidaySeederService(IHolidayRepository holidayRepository, ILogger<HolidaySeederService> logger)
        {
            _holidayRepository = holidayRepository;
            _logger = logger;
        }

        public async Task SeedVietnamHolidaysAsync(int year)
        {
            try
            {
                var holidays = GetVietnamHolidays(year);

                foreach (var holiday in holidays)
                {
                    // Check if holiday already exists
                    var existing = await _holidayRepository.GetHolidayInfoAsync(holiday.Date, "VN");
                    if (existing == null)
                    {
                        await _holidayRepository.AddHolidayAsync(holiday);
                        _logger.LogInformation("Added holiday: {HolidayName} on {Date}", holiday.Name, holiday.Date);
                    }
                }

                _logger.LogInformation("Successfully seeded Vietnam holidays for year {Year}", year);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding Vietnam holidays for year {Year}", year);
                throw;
            }
        }

        public async Task SeedMultipleYearsAsync(int startYear, int endYear)
        {
            for (int year = startYear; year <= endYear; year++)
            {
                await SeedVietnamHolidaysAsync(year);
            }
        }

        private List<Holiday> GetVietnamHolidays(int year)
        {
            var holidays = new List<Holiday>
            {
                // Fixed holidays
                new Holiday
                {
                    Name = "New Year's Day",
                    Date = new DateTime(year, 1, 1),
                    Description = "International New Year's Day",
                    IsRecurring = true,
                    CountryCode = "VN",
                    IsActive = true
                },
                new Holiday
                {
                    Name = "Victory Day",
                    Date = new DateTime(year, 4, 30),
                    Description = "Reunification Day / Liberation Day",
                    IsRecurring = true,
                    CountryCode = "VN",
                    IsActive = true
                },
                new Holiday
                {
                    Name = "Labor Day",
                    Date = new DateTime(year, 5, 1),
                    Description = "International Workers' Day",
                    IsRecurring = true,
                    CountryCode = "VN",
                    IsActive = true
                },
                new Holiday
                {
                    Name = "National Day",
                    Date = new DateTime(year, 9, 2),
                    Description = "Vietnam Independence Day",
                    IsRecurring = true,
                    CountryCode = "VN",
                    IsActive = true
                }
            };

            // Add Lunar Calendar holidays (approximate dates - in real implementation, you'd calculate these properly)
            var lunarNewYear = GetLunarNewYearDate(year);
            if (lunarNewYear.HasValue)
            {
                // Tet holidays (usually 3-5 days)
                for (int i = 0; i < 3; i++)
                {
                    holidays.Add(new Holiday
                    {
                        Name = i == 0 ? "Lunar New Year (Tet)" : $"Tet Holiday Day {i + 1}",
                        Date = lunarNewYear.Value.AddDays(i),
                        Description = "Vietnamese New Year celebration",
                        IsRecurring = true,
                        CountryCode = "VN",
                        IsActive = true
                    });
                }
            }

            // Hung Kings' Commemorations Day (10th day of 3rd lunar month)
            var hungKingsDay = GetHungKingsDay(year);
            if (hungKingsDay.HasValue)
            {
                holidays.Add(new Holiday
                {
                    Name = "Hung Kings' Commemorations Day",
                    Date = hungKingsDay.Value,
                    Description = "National holiday to commemorate the Hung Kings",
                    IsRecurring = true,
                    CountryCode = "VN",
                    IsActive = true
                });
            }

            return holidays;
        }

        private DateTime? GetLunarNewYearDate(int year)
        {
            // This is a simplified mapping. In a real implementation, you'd use proper lunar calendar calculations
            // or integrate with a lunar calendar API
            var lunarNewYearDates = new Dictionary<int, DateTime>
            {
                { 2024, new DateTime(2024, 2, 10) },
                { 2025, new DateTime(2025, 1, 29) },
                { 2026, new DateTime(2026, 2, 17) },
                { 2027, new DateTime(2027, 2, 6) },
                { 2028, new DateTime(2028, 1, 26) }
            };

            return lunarNewYearDates.ContainsKey(year) ? lunarNewYearDates[year] : null;
        }

        private DateTime? GetHungKingsDay(int year)
        {
            // This is a simplified mapping. In a real implementation, you'd calculate the 10th day of 3rd lunar month
            var hungKingsDates = new Dictionary<int, DateTime>
            {
                { 2024, new DateTime(2024, 4, 18) },
                { 2025, new DateTime(2025, 4, 7) },
                { 2026, new DateTime(2026, 4, 27) },
                { 2027, new DateTime(2027, 4, 16) },
                { 2028, new DateTime(2028, 4, 4) }
            };

            return hungKingsDates.ContainsKey(year) ? hungKingsDates[year] : null;
        }
    }
}

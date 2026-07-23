using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IHolidaySeederService
    {
        Task SeedVietnamHolidaysAsync(int year);
        Task SeedMultipleYearsAsync(int startYear, int endYear);
    }
}

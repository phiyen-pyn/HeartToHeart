using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class SetCounselorGeneralAvailabilityDto
    {
        public bool IsAvailable { get; set; }
        public string? Reason { get; set; }
    }
}

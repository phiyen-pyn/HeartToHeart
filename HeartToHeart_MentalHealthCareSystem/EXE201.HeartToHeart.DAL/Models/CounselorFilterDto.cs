using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CounselorFilterDto
    {
        public string? Specialization { get; set; }
        public bool? OnlyAvailable { get; set; }
        public bool? OnlyVerified { get; set; }
    }
}

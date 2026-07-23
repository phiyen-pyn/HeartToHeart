using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CounselorStatusDto
    {
        public Guid CounselorId { get; set; }
        public bool Exists { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsUserActive { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

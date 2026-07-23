using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class AnonymousPostDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsReported { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

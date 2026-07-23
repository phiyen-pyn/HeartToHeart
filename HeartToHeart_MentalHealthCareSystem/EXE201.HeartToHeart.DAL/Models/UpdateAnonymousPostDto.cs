using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class UpdateAnonymousPostDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;
        public bool IsReported { get; set; } = false;
    }
}

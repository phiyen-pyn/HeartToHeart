using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class UpdateDiaryEntryDto
    {
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string? Title { get; set; }

        [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters")]
        public string? Content { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CreateCommentDto
    {
        [Required]
        public Guid PostId { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}

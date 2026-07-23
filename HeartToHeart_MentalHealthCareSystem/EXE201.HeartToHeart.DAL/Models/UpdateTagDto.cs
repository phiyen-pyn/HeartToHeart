using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class UpdateTagDto
    {
        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }
    }
}

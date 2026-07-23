using EXE201.HeartToHeart.DAL.Entities.Base;
using EXE201.HeartToHeart.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class BlogComment : AuditableEntityBase
    {
        public Guid BlogId { get; set; }
        public Guid? UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public Guid? ParentCommentId { get; set; } // For nested comments

        public bool IsApproved { get; set; } = true;

        // Navigation Properties
        public virtual Blog Blog { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
        public virtual BlogComment? ParentComment { get; set; }
        public virtual ICollection<BlogComment> Replies { get; set; } = new List<BlogComment>();
    }
}

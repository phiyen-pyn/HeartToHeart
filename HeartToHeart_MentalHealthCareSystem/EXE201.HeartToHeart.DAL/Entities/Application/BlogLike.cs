using EXE201.HeartToHeart.DAL.Entities.Base;
using EXE201.HeartToHeart.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class BlogLike : EntityBase
    {
        public Guid BlogId { get; set; }
        public Guid UserId { get; set; }

        // Navigation Properties
        public virtual Blog Blog { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}

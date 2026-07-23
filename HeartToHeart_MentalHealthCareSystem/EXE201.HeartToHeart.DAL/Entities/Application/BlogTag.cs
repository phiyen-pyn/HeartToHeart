using EXE201.HeartToHeart.DAL.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class BlogTag : EntityBase
    {
        public Guid BlogId { get; set; }
        public Guid TagId { get; set; }

        // Navigation Properties
        public virtual Blog Blog { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}

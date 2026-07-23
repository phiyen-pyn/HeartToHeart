using EXE201.HeartToHeart.DAL.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class MediaContentTag : EntityBase
    {
        public Guid MediaContentId { get; set; }
        public Guid TagId { get; set; }

        // Navigation Properties
        public virtual MediaContent MediaContent { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserGroups.Models
{
    public class Group
    {
        public Group()
        {
            Members = new List<GroupMember>();
        }

        [Key]
        public virtual int Id { get; set; }

        [Required]
        public virtual string Name { get; set; }

        [Required]
        public virtual IList<GroupMember> Members { get; set; }
    }
}

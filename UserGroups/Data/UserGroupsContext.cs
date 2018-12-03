using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UserGroups.Models
{
    public class UserGroupsContext : DbContext
    {
        public UserGroupsContext (DbContextOptions<UserGroupsContext> options)
            : base(options)
        {
        }

        public DbSet<UserGroups.Models.Group> Group { get; set; }
    }
}

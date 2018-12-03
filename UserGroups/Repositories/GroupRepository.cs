using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserGroups.Models;

namespace UserGroups.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly UserGroupsContext context;

        public GroupRepository(UserGroupsContext context)
        {
            this.context = context;
        }

        public async Task<List<Group>> GetAllAsync()
        {
            return await context.Group.ToListAsync();
        }

        public async Task<Group> GetByIdAsync(int id)
        {
            return await context.Group.Include("Members").SingleOrDefaultAsync(g => g.Id == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserGroups.Models;

namespace UserGroups.Repositories
{
    public class GroupMemberRepository : IGroupMemberRepository
    {
        private readonly UserGroupsContext context;

        public GroupMemberRepository(UserGroupsContext context)
        {
            this.context = context;
        }

        public async Task<GroupMember> GetByUserIdAsync(string userId)
        {
            return await context.GroupMember.SingleOrDefaultAsync(gm => gm.UserId == userId);
        }
    }
}

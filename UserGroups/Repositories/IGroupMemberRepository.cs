using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserGroups.Models;

namespace UserGroups.Repositories
{
    public interface IGroupMemberRepository
    {
        Task<GroupMember> GetByUserIdAsync(string userId);
    }
}

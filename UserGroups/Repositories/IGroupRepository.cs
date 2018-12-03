using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserGroups.Models;

namespace UserGroups.Repositories
{
    public interface IGroupRepository
    {
        Task<Group> GetByIdAsync(int id);

        Task<List<Group>> GetAllAsync();
    }
}

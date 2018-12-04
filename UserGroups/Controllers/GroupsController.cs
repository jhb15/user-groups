using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserGroups.Models;
using UserGroups.Repositories;

namespace UserGroups.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupRepository groupRepository;
        private readonly IGroupMemberRepository groupMemberRepository;

        public GroupsController(IGroupRepository groupRepository, IGroupMemberRepository groupMemberRepository)
        {
            this.groupRepository = groupRepository;
            this.groupMemberRepository = groupMemberRepository;
        }

        // GET: api/Groups
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await groupRepository.GetAllAsync();
            return Ok(groups);
        }

        // GET: api/Groups/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetGroup([FromRoute] int id)
        {
            var group = await groupRepository.GetByIdAsync(id);

            if (group == null)
            {
                return NotFound();
            }

            return Ok(group);
        }

        // GET: api/Groups/ForUser/{userId}
        [HttpGet("ForUser/{userId}")]
        public async Task<IActionResult> GetGroupForUser([FromRoute] string userId)
        {
            var groupMember = await groupMemberRepository.GetByUserIdAsync(userId);

            if (groupMember == null)
            {
                return NotFound();
            }

            var group = await groupRepository.GetByIdAsync(groupMember.GroupId);

            if (group == null)
            {
                return NotFound();
            }

            return Ok(group);
        }
    }
}
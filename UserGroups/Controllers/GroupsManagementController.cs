using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserGroups.Models;
using UserGroups.Repositories;

namespace UserGroups.Controllers
{
    [Authorize(AuthenticationSchemes = "oidc")]
    public class GroupsManagementController : Controller
    {
        private readonly IGroupRepository groupRepository;
        private readonly IGroupMemberRepository groupMemberRepository;

        public GroupsManagementController(IGroupRepository groupRepository, IGroupMemberRepository groupMemberRepository)
        {
            this.groupRepository = groupRepository;
            this.groupMemberRepository = groupMemberRepository;
        }

        // GET: GroupsManagement
        public async Task<IActionResult> Index()
        {
            return View(await groupRepository.GetAllAsync());
        }

        // GET: GroupsManagement/Create
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: GroupsManagement/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,Name")] Group group)
        {
            if (ModelState.IsValid)
            {
                await groupRepository.AddAsync(group);
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // GET: GroupsManagement/Edit/5
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await groupRepository.GetByIdAsync(id.Value);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        // POST: GroupsManagement/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Group group)
        {
            if (id != group.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await groupRepository.UpdateAsync(group);
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // GET: GroupsManagement/Delete/5
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await groupRepository.GetByIdAsync(id.Value);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: GroupsManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await groupRepository.GetByIdAsync(id);
            await groupRepository.DeleteAsync(group);
            return RedirectToAction(nameof(Index));
        }

        // GET: GroupsManagement/Join/{id}
        [HttpGet]
        public async Task<IActionResult> Join(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await groupRepository.GetByIdAsync(id.Value);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: GroupsManagement/Join/{id}
        [HttpPost, ActionName("Join")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinConfirmed(int id)
        {
            var userId = User.Claims.Single(c => c.Type == "sub");
            var groupMember = await groupMemberRepository.GetByUserIdAsync(userId.Value);
            var group = await groupRepository.GetByIdAsync(id);

            if (groupMember != null)
            {
                ViewData["Error"] = "You're already a member of a group.";
                return View(group);
            }

            groupMember = new GroupMember { UserId = userId.Value, GroupId = group.Id };
            group.Members.Add(groupMember);
            await groupRepository.UpdateAsync(group);

            return RedirectToAction(nameof(Index));
        }

        // GET: GroupsManagement/Leave/{id}
        [HttpGet]
        public async Task<IActionResult> Leave(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await groupRepository.GetByIdAsync(id.Value);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: GroupsManagement/Leave/{id}
        [HttpPost, ActionName("Leave")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveConfirmed(int id)
        {
            var userId = User.Claims.Single(c => c.Type == "sub");
            var group = await groupRepository.GetByIdAsync(id);
            var groupMember = group.Members.SingleOrDefault(gm => gm.UserId == userId.Value);

            if (groupMember == null)
            {
                ViewData["Error"] = "You're not a memeber of this group.";
                return View(group);
            }

            group.Members.Remove(groupMember);
            await groupRepository.UpdateAsync(group);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteMember(int groupId, int memberId)
        {
            var group = await groupRepository.GetByIdAsync(groupId);
            var groupMemeber = group.Members.SingleOrDefault(gm => gm.Id == memberId);
            group.Members.Remove(groupMemeber);
            await groupRepository.UpdateAsync(group);

            return View(nameof(Edit), group);
        }
    }
}

using AberFitnessAuditLogger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserGroups.Models;
using UserGroups.Repositories;
using UserGroups.Services;

namespace UserGroups.Controllers
{
    [Authorize(AuthenticationSchemes = "oidc")]
    public class GroupsManagementController : Controller
    {
        private readonly IGroupRepository groupRepository;
        private readonly IGroupMemberRepository groupMemberRepository;
        private readonly IGatekeeperApiClient gatekeeperApiClient;
        private readonly IAuditLogger auditLogger;


        public GroupsManagementController(IGroupRepository groupRepository, IGroupMemberRepository groupMemberRepository, IGatekeeperApiClient gatekeeperApiClient, IAuditLogger auditLogger)
        {
            this.groupRepository = groupRepository;
            this.groupMemberRepository = groupMemberRepository;
            this.gatekeeperApiClient = gatekeeperApiClient;
            this.auditLogger = auditLogger;
        }

        private string CurrentUserId()
        { 
            try
            {
                return User.Claims.Where(c => c.Type == "sub").FirstOrDefault().Value;
            } catch (NullReferenceException)
            {
                return "Unknown";
            }
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,Name")] Group group)
        {
            if (ModelState.IsValid)
            {
                await auditLogger.log(CurrentUserId(), $"Created group {group.Name}");
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

            var response = await gatekeeperApiClient.PostAsync("api/Users/Batch", group.Members.Select(m => m.UserId).ToArray());
            if(response.IsSuccessStatusCode)
            {
                ViewData["Users"] = JsonConvert.DeserializeObject<User[]>(response.Content.ReadAsStringAsync().Result);
            } else
            {
                ViewData["Users"] = new User[0];
            }

            return View(group);
        }

        // POST: GroupsManagement/Edit/{id}
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
                await auditLogger.log(CurrentUserId(), $"Edited group {group.Id}, {group.Name}");
                await groupRepository.UpdateAsync(group);
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // GET: GroupsManagement/Delete/{id}
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

        // POST: GroupsManagement/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await groupRepository.GetByIdAsync(id);
            await auditLogger.log(CurrentUserId(), $"Deleted group {group.Id}, {group.Name}");
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
            await auditLogger.log(CurrentUserId(), $"Joined group {group.Id}, {group.Name}");

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
            await auditLogger.log(CurrentUserId(), $"Left group {group.Id}, {group.Name}");

            return RedirectToAction(nameof(Index));
        }

        // GET: GroupsManagement/DeleteMember?memberId={memberId}&groupId={groupId}
        [HttpGet]
        [Authorize(AuthenticationSchemes = "oidc", Policy = "Administrator")]
        public async Task<IActionResult> DeleteMember(int groupId, int memberId)
        {
            var group = await groupRepository.GetByIdAsync(groupId);
            var groupMemeber = group.Members.SingleOrDefault(gm => gm.Id == memberId);
            group.Members.Remove(groupMemeber);
            await groupRepository.UpdateAsync(group);
            await auditLogger.log(groupMemeber.UserId, $"Removed from {group.Id}, {group.Name} by {CurrentUserId()}");

            return RedirectToAction(nameof(Edit), group);
        }
    }
}

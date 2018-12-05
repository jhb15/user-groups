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

        public GroupsManagementController(IGroupRepository groupRepository)
        {
            this.groupRepository = groupRepository;
        }

        // GET: GroupsManagement
        public async Task<IActionResult> Index()
        {
            return View(await groupRepository.GetAllAsync());
        }

        // GET: GroupsManagement/Details/5
        public async Task<IActionResult> Details(int? id)
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
        public async Task<IActionResult> Join(int id)
        {
            throw new NotImplementedException();
        }

        // POST: GroupsManagement/Join/{id}
        [HttpPost, ActionName("Join")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinConfirmed(int id)
        {
            throw new NotImplementedException();
        }
    }
}

using AberFitnessAuditLogger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using UserGroups.Controllers;
using UserGroups.Models;
using UserGroups.Repositories;
using UserGroups.Services;
using UserGroupsTest.TestUtils;
using Xunit;

namespace UserGroupsTest.Controllers
{
    public class GroupsManagementController_Test
    {
        private Mock<IGroupRepository> groupRepository;
        private Mock<IGroupMemberRepository> groupMemberRepository;
        private Mock<IGatekeeperApiClient> gatekeeperApiClient;
        private Mock<IAuditLogger> auditLogger;
        private GroupsManagementController controller;

        public HttpStatusCode HttpsStatusCode { get; private set; }

        public GroupsManagementController_Test()
        {
            groupRepository = new Mock<IGroupRepository>();
            groupMemberRepository = new Mock<IGroupMemberRepository>();
            gatekeeperApiClient = new Mock<IGatekeeperApiClient>();
            auditLogger = new Mock<IAuditLogger>();
            controller = new GroupsManagementController(groupRepository.Object, groupMemberRepository.Object, gatekeeperApiClient.Object, auditLogger.Object);
        }

        [Fact]
        public async void Index_GetsAllGroups()
        {
            var groups = GroupGenerator.CreateList();
            groupRepository.Setup(gr => gr.GetAllAsync()).ReturnsAsync(groups);
            var result = await controller.Index();
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsType<List<Group>>(viewResult.Model);
            Assert.Null(viewResult.ViewName);
            groupRepository.Verify();
        }

        [Fact]
        public void Create_ReturnsCorrectView()
        {
            var result = controller.Create();
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Null(viewResult.Model);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async void Create_ReturnsViewIfGroupInvalid()
        {
            controller.ModelState.AddModelError("any", "error");
            var group = GroupGenerator.Create();
            var result = await controller.Create(group);
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Equal(group, viewResult.Model);
            Assert.Null(viewResult.ViewName);
            groupRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async void Create_AddsGroup()
        {
            var group = GroupGenerator.Create();
            groupRepository.Setup(gr => gr.AddAsync(group)).ReturnsAsync(group);
            var result = await controller.Create(group);
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
            groupRepository.Verify();
        }

        [Fact]
        public async void Edit_ReturnsNotFoundWithNullId()
        {
            var result = await controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void Edit_ReturnsNotFoundWithInvalidId()
        {
            groupRepository.Setup(gr => gr.GetByIdAsync(1)).ReturnsAsync((Group)null);
            var result = await controller.Edit(1);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void Edit_ReturnsCorrectView()
        {
            var group = GroupGenerator.Create();
            group.Members = GroupMemberGenerator.CreateList();
            var apiResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[{\"email\":\"test@example.com\"}]")
            };
            groupRepository.Setup(gr => gr.GetByIdAsync(group.Id)).ReturnsAsync(group);
            gatekeeperApiClient.Setup(gap => gap.PostAsync("api/Users/Batch", It.IsAny<object>())).ReturnsAsync(apiResponse);
            var result = await controller.Edit(group.Id);
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Equal(group, viewResult.Model);
            Assert.Null(viewResult.ViewName);
            Assert.Contains("Users", viewResult.ViewData);
            groupRepository.Verify();
            gatekeeperApiClient.Verify();
        }

        [Fact]
        public async void Edit_ReturnsNotFoundOnIdMismatch()
        {
            var group = GroupGenerator.Create();
            var invalidId = group.Id + 1;
            var result = await controller.Edit(invalidId, group);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void Edit_ReturnsViewIfGroupInvalid()
        {
            controller.ModelState.AddModelError("any", "error");
            var group = GroupGenerator.Create();
            var result = await controller.Edit(group.Id, group);
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Equal(group, viewResult.Model);
            Assert.Null(viewResult.ViewName);
            groupRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async void Edit_UpdatesGroup()
        {
            var group = GroupGenerator.Create();
            groupRepository.Setup(gr => gr.UpdateAsync(group)).ReturnsAsync(group);
            var result = await controller.Edit(group.Id, group);
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
            groupRepository.Verify();
        }

        [Fact]
        public async void Delete_ReturnsNotFoundOnNullId()
        {
            var result = await controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void Delete_ReturnsNotFoundOnInvalidId()
        {
            groupRepository.Setup(gr => gr.GetByIdAsync(1)).ReturnsAsync((Group)null);
            var result = await controller.Delete(1);
            Assert.IsType<NotFoundResult>(result);
            groupRepository.Verify();
        }

        [Fact]
        public async void Delete_ShowsCorrectView()
        {
            var group = GroupGenerator.Create();
            groupRepository.Setup(gr => gr.GetByIdAsync(group.Id)).ReturnsAsync(group);
            var result = await controller.Delete(group.Id);
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Null(viewResult.ViewName);
            Assert.Equal(group, viewResult.Model);
            groupRepository.Verify();
        }

        [Fact]
        public async void DeleteConfirmed_DeletesGroup()
        {
            var group = GroupGenerator.Create();
            groupRepository.Setup(gr => gr.GetByIdAsync(group.Id)).ReturnsAsync(group);
            groupRepository.Setup(gr => gr.DeleteAsync(group)).ReturnsAsync(group);
            var result = await controller.DeleteConfirmed(group.Id);
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
            groupRepository.Verify();
        }

        [Fact]
        public async void Join_ReturnsNotFoundOnNullId()
        {
            var result = await controller.Join(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void Join_ReturnsNotFoundOnInvalidId()
        {
            groupRepository.Setup(gr => gr.GetByIdAsync(1)).ReturnsAsync((Group)null);
            var result = await controller.Join(1);
            Assert.IsType<NotFoundResult>(result);
            groupRepository.Verify();
        }

        [Fact]
        public async void Join_ShowsCorrectView()
        {
            var group = GroupGenerator.Create();
            groupRepository.Setup(gr => gr.GetByIdAsync(group.Id)).ReturnsAsync(group);
            var result = await controller.Join(group.Id);
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Null(viewResult.ViewName);
            Assert.Equal(group, viewResult.Model);
            groupRepository.Verify();
        }

        [Fact]
        public async void Leave_ReturnsNotFoundOnNullId()
        {
            var result = await controller.Leave(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void Leave_ReturnsNotFoundOnInvalidId()
        {
            groupRepository.Setup(gr => gr.GetByIdAsync(1)).ReturnsAsync((Group)null);
            var result = await controller.Leave(1);
            Assert.IsType<NotFoundResult>(result);
            groupRepository.Verify();
        }

        [Fact]
        public async void Leave_ShowsCorrectView()
        {
            var group = GroupGenerator.Create();
            groupRepository.Setup(gr => gr.GetByIdAsync(group.Id)).ReturnsAsync(group);
            var result = await controller.Leave(group.Id);
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Null(viewResult.ViewName);
            Assert.Equal(group, viewResult.Model);
            groupRepository.Verify();
        }

        [Fact]
        public async void DeleteMember_DeletesMember()
        {
            var group = GroupGenerator.Create();
            var groupMember = GroupMemberGenerator.Create(groupId: group.Id);
            group.Members.Clear();
            group.Members.Add(groupMember);
            groupRepository.Setup(gr => gr.GetByIdAsync(group.Id)).ReturnsAsync(group);
            groupRepository.Setup(gr => gr.UpdateAsync(group)).ReturnsAsync(group);
            var result = await controller.DeleteMember(group.Id, groupMember.Id);
            Assert.DoesNotContain(groupMember, group.Members);
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.Equal(nameof(controller.Edit), redirectResult.ActionName);
            groupRepository.Verify();
        }
    }
}

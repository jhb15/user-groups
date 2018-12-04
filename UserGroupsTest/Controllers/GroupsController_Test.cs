using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using UserGroups.Controllers;
using UserGroups.Models;
using UserGroups.Repositories;
using UserGroupsTest.TestUtils;
using Xunit;

namespace UserGroupsTest.Controllers
{
    public class GroupsController_Test
    {
        private readonly Mock<IGroupRepository> groupRepository;
        private readonly Mock<IGroupMemberRepository> groupMemberRepository;
        private readonly GroupsController controller;

        public GroupsController_Test()
        {
            groupRepository = new Mock<IGroupRepository>();
            groupMemberRepository = new Mock<IGroupMemberRepository>();
            controller = new GroupsController(groupRepository.Object, groupMemberRepository.Object);
        }

        [Fact]
        public async void GetGroups_ReturnsAllGroups()
        {
            var groups = GroupGenerator.CreateList();
            groupRepository.Setup(gr => gr.GetAllAsync()).ReturnsAsync(groups).Verifiable();
            var result = await controller.GetGroups();
            Assert.IsType<OkObjectResult>(result);
            var content = result as OkObjectResult;
            Assert.IsType<List<Group>>(content.Value);
            Assert.Equal(groups, content.Value);
            groupRepository.Verify();
            groupRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async void GetGroup_ReturnsNotFound()
        {
            groupRepository.Setup(gr => gr.GetByIdAsync(1)).ReturnsAsync((Group)null).Verifiable();
            var result = await controller.GetGroup(1);
            Assert.IsType<NotFoundResult>(result);
            groupRepository.Verify();
            groupRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async void GetGroup_ReturnsCorrectGroup()
        {
            var group = GroupGenerator.Create();
            groupRepository.Setup(gr => gr.GetByIdAsync(group.Id)).ReturnsAsync(group).Verifiable();
            var result = await controller.GetGroup(group.Id);
            Assert.IsType<OkObjectResult>(result);
            var content = result as OkObjectResult;
            Assert.IsType<Group>(content.Value);
            Assert.Equal(group, content.Value);
            groupRepository.Verify();
            groupRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async void GetGroupForUser_ReturnsNotFoundOnInvalidUserId()
        {
            groupMemberRepository.Setup(gmr => gmr.GetByUserIdAsync("someId")).ReturnsAsync((GroupMember)null).Verifiable();
            var result = await controller.GetGroupForUser("someId");
            Assert.IsType<NotFoundResult>(result);
            groupMemberRepository.Verify();
            groupMemberRepository.VerifyNoOtherCalls();
            groupRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async void GetGroupForUser_ReturnsNotFoundWhenNotAMember()
        {
            var groupMember = GroupMemberGenerator.Create();
            groupMemberRepository.Setup(gmr => gmr.GetByUserIdAsync(groupMember.UserId)).ReturnsAsync(groupMember).Verifiable();
            groupRepository.Setup(gr => gr.GetByIdAsync(groupMember.GroupId)).ReturnsAsync((Group)null).Verifiable();
            var result = await controller.GetGroupForUser(groupMember.UserId);
            Assert.IsType<NotFoundResult>(result);
            groupMemberRepository.Verify();
            groupMemberRepository.VerifyNoOtherCalls();
            groupRepository.Verify();
            groupRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async void GetGroupForUser_ReturnsCorrectGroup()
        {
            var group = GroupGenerator.Create();
            var groupMember = GroupMemberGenerator.Create(groupId: group.Id);
            groupMemberRepository.Setup(gmr => gmr.GetByUserIdAsync(groupMember.UserId)).ReturnsAsync(groupMember).Verifiable();
            groupRepository.Setup(gr => gr.GetByIdAsync(groupMember.GroupId)).ReturnsAsync(group).Verifiable();
            var result = await controller.GetGroupForUser(groupMember.UserId);
            Assert.IsType<OkObjectResult>(result);
            var content = result as OkObjectResult;
            Assert.IsType<Group>(content.Value);
            Assert.Equal(group, content.Value);
            groupMemberRepository.Verify();
            groupMemberRepository.VerifyNoOtherCalls();
            groupRepository.Verify();
            groupRepository.VerifyNoOtherCalls();
        }
    }
}

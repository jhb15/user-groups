using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UserGroups.Models;
using UserGroups.Repositories;
using UserGroupsTest.TestUtils;
using Xunit;

namespace UserGroupsTest.Repositories
{
    public class GroupMemberRepository_Test
    {
        private static readonly Random random = new Random();
        private readonly DbContextOptions<UserGroupsContext> contextOptions;

        public GroupMemberRepository_Test()
        {
            contextOptions = new DbContextOptionsBuilder<UserGroupsContext>()
                .UseInMemoryDatabase($"rand_db_name_{random.Next()}")
                .Options;
        }

        [Fact]
        public async void GetByUserIdAsync_ReturnsCorrectItems()
        {
            var list = GroupMemberGenerator.CreateList(5);
            var expected = list[2];
            using (var context = new UserGroupsContext(contextOptions))
            {
                context.Database.EnsureCreated();
                context.GroupMember.AddRange(list);
                context.SaveChanges();
                Assert.Equal(list.Count, await context.GroupMember.CountAsync());
                var repository = new GroupMemberRepository(context);
                var activity = await repository.GetByUserIdAsync(expected.UserId);
                Assert.IsType<GroupMember>(activity);
                Assert.Equal(expected, activity);
            }
        }
    }
}

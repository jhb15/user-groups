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
    public class GroupRepository_Test
    {
        private static readonly Random random = new Random();
        private readonly DbContextOptions<UserGroupsContext> contextOptions;

        public GroupRepository_Test()
        {
            contextOptions = new DbContextOptionsBuilder<UserGroupsContext>()
                .UseInMemoryDatabase($"rand_db_name_{random.Next()}")
                .Options;
        }

        [Fact]
        public async void GetAllAsync_ReturnsAllGroups()
        {
            var expectedGroups = GroupGenerator.CreateList();
            using (var context = new UserGroupsContext(contextOptions))
            {
                context.Database.EnsureCreated();
                context.Group.AddRange(expectedGroups);
                context.SaveChanges();
                Assert.Equal(expectedGroups.Count, await context.Group.CountAsync());
                var repository = new GroupRepository(context);
                var groups = await repository.GetAllAsync();
                Assert.IsType<List<Group>>(groups);
                Assert.Equal(expectedGroups.Count, groups.Count);
                Assert.Equal(expectedGroups, groups);
            }
        }

        [Fact]
        public async void GetByIdAsync_ReturnsCorrectGroup()
        {
            var list = GroupGenerator.CreateList(5);
            var expected = list[2];
            using (var context = new UserGroupsContext(contextOptions))
            {
                context.Database.EnsureCreated();
                context.Group.AddRange(list);
                context.SaveChanges();
                Assert.Equal(list.Count, await context.Group.CountAsync());
                var repository = new GroupRepository(context);
                var group = await repository.GetByIdAsync(expected.Id);
                Assert.IsType<Group>(group);
                Assert.Equal(expected, group);
            }
        }
    }
}

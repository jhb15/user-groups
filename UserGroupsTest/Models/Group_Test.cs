using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UserGroups.Models;
using UserGroupsTest.TestUtils;
using Xunit;

namespace UserGroupsTest.Models
{
    public class Group_Test
    {
        [Fact]
        public void Group_RequiresAName()
        {
            Group g = new Group()
            {
                Id = 0,
                Name = null,
                Members = new List<GroupMember>()
            };

            var results = ModelValidator.GetValidation(g);
            Assert.Equal(1, results.Count);
            Assert.Equal(new string[] { "Name" }, results[0].MemberNames);
            Assert.Contains("required", results[0].ErrorMessage);
        }

        [Fact]
        public void Group_RequiresAMembersLIst()
        {
            Group g = new Group()
            {
                Id = 0,
                Name = "Group Name",
                Members = null
            };

            var results = ModelValidator.GetValidation(g);
            Assert.Equal(1, results.Count);
            Assert.Equal(new string[] { "Members" }, results[0].MemberNames);
            Assert.Contains("required", results[0].ErrorMessage);
        }
    }
}

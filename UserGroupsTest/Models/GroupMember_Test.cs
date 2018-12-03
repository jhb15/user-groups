using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UserGroups.Models;
using UserGroupsTest.TestUtils;
using Xunit;

namespace UserGroupsTest.Models
{
    public class GroupMember_Test
    {
        [Fact]
        public void GroupMember_RequiresAUserId()
        {
            GroupMember gm = new GroupMember()
            {
                Id = 0,
                UserId = null,
                Type = MemberType.Member
            };

            var results = ModelValidator.GetValidation(gm);
            Assert.Equal(1, results.Count);
            Assert.Equal(new string[] { "UserId" }, results[0].MemberNames);
            Assert.Contains("required", results[0].ErrorMessage);
        }

        [Fact]
        public void GroupMember_UserIdMinLength36()
        {
            GroupMember gm = new GroupMember()
            {
                Id = 0,
                UserId = "a".PadLeft(35, 'a'),
                Type = MemberType.Member
            };

            var results = ModelValidator.GetValidation(gm);
            Assert.Equal(1, results.Count);
            Assert.Equal(new string[] { "UserId" }, results[0].MemberNames);
            Assert.Contains("minimum length", results[0].ErrorMessage);
        }

        [Fact]
        public void GroupMember_UserIdMaxLength36()
        {
            GroupMember gm = new GroupMember()
            {
                Id = 0,
                UserId = "a".PadLeft(37, 'a'),
                Type = MemberType.Member
            };

            var results = ModelValidator.GetValidation(gm);
            Assert.Equal(1, results.Count);
            Assert.Equal(new string[] { "UserId" }, results[0].MemberNames);
            Assert.Contains("maximum length", results[0].ErrorMessage);
        }
    }
}

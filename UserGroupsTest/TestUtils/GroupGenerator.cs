using System;
using System.Collections.Generic;
using System.Text;
using UserGroups.Models;

namespace UserGroupsTest.TestUtils
{
    public class GroupGenerator
    {
        public static Group Create(int index = 0)
        {
            return new Group
            {
                Id = index,
                Name = $"group name {index}",
                Members = GroupMemberGenerator.CreateList(groupId: index)
            };
        }

        public static List<Group> CreateList(int length = 5)
        {
            List<Group> list = new List<Group>();
            for (var i = 1; i <= length; i++)
            {
                list.Add(Create(i));
            }
            return list;
        }
    }
}

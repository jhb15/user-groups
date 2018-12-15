using System;
using System.Collections.Generic;
using System.Text;
using UserGroups.Models;

namespace UserGroupsTest.TestUtils
{
    public class GroupMemberGenerator
    {
        public static GroupMember Create(int index = 0, int groupId = 0)
        {
            return new GroupMember
            {
                UserId = $"{index}".PadLeft(36),
                GroupId = groupId
            };
        }

        public static List<GroupMember> CreateList(int length = 5, int groupId = 0)
        {
            List<GroupMember> list = new List<GroupMember>();
            for (var i = 0; i < length; i++)
            {
                list.Add(Create(i, groupId));
            }
            return list;
        }
    }
}

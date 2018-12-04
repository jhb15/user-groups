using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UserGroups.Controllers;
using Xunit;

namespace UserGroupsTest.Controllers
{
    public class StatusController_Test
    {
        [Fact]
        public void Get_ReturnsOk()
        {
            var controller = new StatusController();
            var result = controller.Get();
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Get_HasCorrectContent()
        {
            var controller = new StatusController();
            var result = controller.Get();
            var content = result as OkObjectResult;
            Assert.Null(content);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using MvcApplication2.Models;

namespace MvcApplication2.Controllers.Tests
{
    [TestFixture]
    public class HomeControllerTest
    {

        [Test]
        public void IndexActionReturnsIndexView()
        {
            string expected = "Index";
            HomeController controller = new HomeController();

            LoginModel log = new LoginModel();
            log.EmailId = "exemplo@hotmail.com";
            log.Password = "123456";

            var result = controller.LogOn(log) as ViewResult;

            Assert.AreEqual(expected, result);
        }
    }
}
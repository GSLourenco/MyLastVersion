using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using MvcApplication2.DataModel;

namespace MvcApplication2.Tests
{
    [TestFixture]
    public class ContactsBusinessTest
    {
        [Test]
        public void GetOneUser()
        {
            List<String> result = ContactBusinessLayer.GetUsers("jorgepint98@hotmail.com");
            int r = 1;

            Assert.AreEqual(r, result.Count);
        }

        [Test]
        public void GetZeroUser()
        {
            List<String> result = ContactBusinessLayer.GetUsers("naoexiste@hotmail.com");
            int r = 0;

            Assert.AreEqual(r, result.Count);
        }

        [Test]
        public void PostNewContactAndDeleteIt()
        {
            int result = ContactBusinessLayer.PostContact("teste", "jorgepint98@hotmail.com");
            int r = 0;

            Assert.Greater(result, r);

            Boolean b = ContactBusinessLayer.DeleteContact("teste", "jorgepint98@hotmail.com");
            Assert.AreEqual(b, false);
        }

        [Test]
        public void PostNewContactThatAlreadyExists()
        {
            int result = ContactBusinessLayer.PostContact("Jorge_S3_mini", "jorgepint98@hotmail.com");
            int r = 0;

            Assert.AreEqual(result, r);
        }

        [Test]
        public void PostAnInvalidContact()
        {
            int result = ContactBusinessLayer.PostContact("alotlotlotlotofcharacters", "jorgepint98@hotmail.com");
            int r = 0;

            Assert.AreEqual(result, r);
        }

        [Test]
        public void DeleteContactThatDoesntExists()
        {
            bool result = ContactBusinessLayer.DeleteContact("alotlotlotlotofcharacters", "jorgepint98@hotmail.com");
            bool r = false;

            Assert.AreEqual(result, r);
        }









    }
}
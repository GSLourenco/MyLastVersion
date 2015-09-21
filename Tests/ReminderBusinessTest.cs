using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using MvcApplication2.DataModel;
using MvcApplication2.Models;

namespace MvcApplication2.Tests
{
    [TestFixture]
    public class ReminderBusinessTest
    {
        [Test]
        public void GetReminders()
        {
            IEnumerable<Reminder> result = ReminderBusinessLayer.GetReminders("jorgepint98@hotmail.com", "Jorge_S3_mini");
            int r = 4;

            Assert.AreEqual(r, result.Count());
        }

        [Test]
        public void GetRemindersFromUserDoesntExists()
        {
            IEnumerable<Reminder> result = ReminderBusinessLayer.GetReminders("jorgepint98@hotmail.com", "JJorge_S3_mini");
            int r = 0;

            Assert.AreEqual(result, null);
        }

        [Test]
        public void addReminderAndDeleteIt()
        {
            Reminder r = new Reminder();
            r.contact = "Jorge_S3_mini";
            r.description = "aaa";
            r.title = "bbb";
            r.date = "2020-10-10";
            r.time = "20:30";
            r.repeatingDays = new Boolean[] { false, false, false, false, false, false, false };
            r.urls = "[]";

            int result = ReminderBusinessLayer.addReminder(r, "jorgepint98@hotmail.com");
            int res = 0;

            Assert.Greater(result, res);

            Boolean d = ReminderBusinessLayer.DeleteReminder("jorgepint98@hotmail.com", result);
            Assert.AreEqual(d, true);

        }

        [Test]
        public void addReminderWithInvalidDate()
        {
            Reminder r = new Reminder();
            r.contact = "Jorge_S3_mini";
            r.description = "aaa";
            r.title = "bbb";
            r.date = "2010-10-10";
            r.time = "20:30";
            r.repeatingDays = new Boolean[] { false, false, false, false, false, false, false };
            r.urls = "[]";

            int result = ReminderBusinessLayer.addReminder(r, "jorgepint98@hotmail.com");
            int res = -2;

            Assert.AreEqual(result, res);
        }

        [Test]
        public void addReminderWithInvalidContact()
        {
            Reminder r = new Reminder();
            r.contact = "naoexiste";
            r.description = "aaa";
            r.title = "bbb";
            r.date = "2020-10-10";
            r.time = "20:30";
            r.repeatingDays = new Boolean[] { false, false, false, false, false, false, false };
            r.urls = "[]";

            int result = ReminderBusinessLayer.addReminder(r, "jorgepint98@hotmail.com");
            int res = -1;

            Assert.AreEqual(result, res);
        }

        [Test]
        public void EditReminder()
        {
            Reminder r = new Reminder();
            r.id = 135;
            r.description = "aaa";
            r.title = "bbb";
            r.date = "2020-10-10";
            r.time = "20:30";
            r.repeatingDays = new Boolean[] { false, false, false, false, false, false, false };
            r.urls = "[]";

            int result = ReminderBusinessLayer.EditReminder(r, "jorgepint98@hotmail.com");
            int res = 2;

            Assert.AreEqual(result, res);
        }

        [Test]
        public void EditReminderWithBadDate()
        {
            Reminder r = new Reminder();
            r.id = 135;
            r.description = "aaa";
            r.title = "bbb";
            r.date = "2010-10-10";
            r.time = "20:30";
            r.repeatingDays = new Boolean[] { false, false, false, false, false, false, false };
            r.urls = "[]";

            int result = ReminderBusinessLayer.EditReminder(r, "jorgepint98@hotmail.com");
            int res = -2;

            Assert.AreEqual(result, res);
        }

        [Test]
        public void EditReminderWithBadId()
        {
            Reminder r = new Reminder();
            r.id = 100;
            r.description = "aaa";
            r.title = "bbb";
            r.date = "2010-10-10";
            r.time = "20:30";
            r.repeatingDays = new Boolean[] { false, false, false, false, false, false, false };
            r.urls = "[]";

            int result = ReminderBusinessLayer.EditReminder(r, "jorgepint98@hotmail.com");
            int res = -1;

            Assert.AreEqual(result, res);
        }

        [Test]
        public void SendRemindersValidation()
        {
            String id = ReminderBusinessLayer.ValidateSendReminders("Jorge_S3_mini", "jorgepint98@hotmail.com");
            int res = 152;

            Assert.AreEqual(id.Length, res);
        }

        [Test]
        public void SendRemindersValidationToAuserThatDoestExists()
        {
            String id = ReminderBusinessLayer.ValidateSendReminders("JJorge_S3_mini", "jorgepint98@hotmail.com");
            String res = null;

            Assert.AreEqual(id, res);
        }


    }
}
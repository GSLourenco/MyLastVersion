using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplication2.DataModel;
using MvcApplication2.Models;
using System.Collections.Generic;

namespace MvcApplication2.DataModel
{
    public static class ReminderBusinessLayer
    {
        public static IEnumerable<Reminder> GetReminders(String email, String user)
        {
            //chech if contact exists 
            int id = PictogramsDb.getContactId(user, email);
            if (id < 0)
                return null;
            return PictogramsDb.getAllReminders(id, email);
        }

        public static IEnumerable<Reminder> GetFilteredReminders(String email, int contactId)
        {
            //chech if contact exists 
            IEnumerable<Reminder> list = PictogramsDb.getAllReminders(contactId, email);
            list = list.Where(r => DateTime.ParseExact(r.date + " " + r.time, "yyyy-M-d HH:mm",
                                                 System.Globalization.CultureInfo.InvariantCulture) > DateTime.UtcNow.AddHours(1));
            //transfer reminders to historic
            PictogramsDb.TransferReminderstoHistorical(list, contactId, email);
            return list;
        }

        public static IEnumerable<Reminder> GetHistoricalReminders(String email, String user)
        {
            int id = PictogramsDb.getContactId(user, email);
            if (id < 0) return null;

            return PictogramsDb.GetHistoricalReminders(email, id);
        }

        public static Reminder GetHistoricalDetailedReminder(String email, int id)
        {
            Reminder r = PictogramsDb.getHistoricalReminder(id, email);
            return r;
        }



        public static Boolean DeleteReminder(String name, int id)
        {
            if (id < 0) return false;

            Reminder r = PictogramsDb.getReminder(id, name);
            if (r == null)
                return false;
            return PictogramsDb.DeleteReminder(id, name);
        }

        public static int addReminder(Reminder r, String email)
        {
            r.daysofweek = Utils.getDaysOfWeekInt(r.repeatingDays);
            DateTime rtime = DateTime.ParseExact(r.date + " " + r.time, "yyyy-M-d HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            if (rtime < DateTime.UtcNow) return -2;
            return PictogramsDb.addReminder(r, email);
        }

        public static String ValidateSendReminders(String user, String email)
        {
            String reg_id = PictogramsDb.getRegisteredId(user, email);

            //check if user exists or is not registered in GCM
            if (reg_id == String.Empty) return null;

            int id = PictogramsDb.getContactId(user, email);
            IEnumerable<Reminder> list = GetReminders(email, user);
            if (list == null || !list.Any()) return null;

            return reg_id;
        }

        public static int EditReminder(Reminder r, String mail)
        {
            //check if the reminder that is being edited is from this user
            if (r.id < 0 || !PictogramsDb.checkIfReminderIsFromThisUser(r.id, mail)) return -1;

            //edit reminder
            r.daysofweek = Utils.getDaysOfWeekInt(r.repeatingDays);
            DateTime rtime = DateTime.ParseExact(r.date + " " + r.time, "yyyy-M-d HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            if (rtime < DateTime.UtcNow.AddHours(1)) return -2;
            PictogramsDb.EditReminder(r);

            return 2;
        }

        public static Reminder GetReminderById(int id, String mail)
        {
            if (id < 0) return null;

            return PictogramsDb.getReminder(id, mail);
        }
    }
}
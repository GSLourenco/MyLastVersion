using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplication2.DataModel;
using MvcApplication2.Models;
using System.Collections.Generic;

namespace MvcApplication2.DataModel
{
    public static class ContactBusinessLayer
    {
        public static List<String> GetUsers(String name)
        {
            return PictogramsDb.getUsersFrom(name);
        }

        public static List<ContactModel> GetUsersModel(String mail)
        {
            return PictogramsDb.getUsersModelFrom(mail);
        }

        public static int PostContact(String contact, String mail)
        {
            int code = 0;
            int idx = PictogramsDb.getContactId(contact, mail);
            if (idx > 0 || contact.Length > 20 || PictogramsDb.getTemporaryCode(mail, contact) != null) return code;

            Random rnd = new Random();
            code = rnd.Next(10000, 99999);
            String status = "Código não inserido";

            while (!PictogramsDb.InsertTemporaryCode(contact, code, status, mail))
            {
                code = rnd.Next(10000, 99999);
            }

            return code;
        }

        public static List<TemporaryCode> GetTemporaryCodes(String mail)
        {
            return PictogramsDb.getTemporaryCodes(mail);
        }

        public static Boolean DeleteContact(String contact, String mail)
        {
            return PictogramsDb.DeleteUser(contact, mail);
        }
    }
}
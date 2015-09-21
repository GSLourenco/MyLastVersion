using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Text;
using System.Security.Principal;
using MvcApplication2.Models;
using MvcApplication2.DataModel;
using MvcApplication2.GCM;

namespace MvcApplication2.Controllers
{
    public class RemindersController : ApiController
    {
        [BasicAuthentication]
        public IEnumerable<Reminder> GetAllReminders()
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String[] credentials = idd.Name.Split(new char[] { ' ' });
            String contact = credentials[0];
            String mail = credentials[1];

            return ReminderBusinessLayer.GetFilteredReminders(mail,Int32.Parse(contact));
        }


        [BasicAuthentication]
        public Reminder GetReminderById(int id)
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String[] credentials = idd.Name.Split(new char[] { ' ' });
            String mail = credentials[1];

            Reminder r = ReminderBusinessLayer.GetReminderById(id, mail);
            if (r == null)
                throw new HttpResponseException(this.Request.CreateResponse(HttpStatusCode.NotFound));
            return r;
        }



        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage PostReminder(Reminder r)
        {

            if (!this.ModelState.IsValid || r == null) //|| !Utils.checkUri(r.urls))
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, ModelState.ToString());

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;

            int _id = ReminderBusinessLayer.addReminder(r, username);
            if (_id == -1) return this.Request.CreateResponse(HttpStatusCode.NotFound, "Can´t add reminder to a user that don´t exist");

            String validate = ReminderBusinessLayer.ValidateSendReminders(r.contact, username);

            if (validate == null){
                return this.Request.CreateResponse(HttpStatusCode.NotFound, "User is not registered in GCM or there is no reminders to send");
            }
           
            //Send GCM message
            String errorcode = ServiceLayer.SendNotification(validate, username);

            if (errorcode != null)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);

        }

        //Testar retirei o contacto
        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage DeleteReminder(int id)
        {
            if (!this.ModelState.IsValid || id < 0)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;

            Boolean delete = ReminderBusinessLayer.DeleteReminder(username, id);
            if (!delete) return new HttpResponseMessage(HttpStatusCode.NotFound);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }


    }
}

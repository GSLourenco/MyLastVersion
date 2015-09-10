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
using System.Web.Mvc;

namespace MvcApplication2.Controllers
{
    public class RemindersController : ApiController
    {
        [BasicAuthentication]
        public IEnumerable<Reminder> getAllReminders()
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String [] credentials = idd.Name.Split(new char []{' '});
            IEnumerable<Reminder> list= PictogramsDb.getAllReminders(Int32.Parse(credentials[0]), credentials[1]);

            if(list.Any())PictogramsDb.TransferReminderstoHistorical(list, Int32.Parse(credentials[0]), credentials[1]);

            return list;
        }

        //mexer depois
        [BasicAuthentication]
        public Reminder getReminderById(int id)
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String[] credentials = idd.Name.Split(new char[] { ' ' });
            //check if reminder is from user

            Reminder r = PictogramsDb.getReminder(id,credentials[1]);
            if(r == null)
                throw new HttpResponseException(this.Request.CreateResponse(HttpStatusCode.NotFound));
            return r;
        }

        [BasicAuthenticationAttributeWithPassword]
        public ActionResult SendReminders(String User)
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String name = idd.Name;

            String reg_id = PictogramsDb.getRegisteredId(User, name);

            if (reg_id == String.Empty) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User is not registered in GCM");
            int id = PictogramsDb.getContactId(User, name);
            IEnumerable<Reminder> list = PictogramsDb.getAllReminders(id, name);
            if (!list.Any()) return new HttpStatusCodeResult(HttpStatusCode.MethodNotAllowed); 

            Sender s = new Sender(Constants.Project_key);
            Message m = new Message.Builder()
                 .collapseKey("Update reminders")
                 .timeToLive(2419200)
                 .delayWhileIdle(true)
                 .dryRun(false)
                 .addData("Email", name)
                 .build();

            Result r = s.sendNoRetry(m, reg_id);
            String errorcode = r.getErrorCodeName();

            if (errorcode != null)
            {
                
                if (errorcode == Constants.ERROR_INVALID_REGISTRATION)
                {
                    //return View("UnregisterUser", (object)User);
                }
                else if (errorcode == Constants.ERROR_NOT_REGISTERED)
                {
                    //return View("RefreshToken", (object)User);
                }
                else if (errorcode == Constants.ERROR_MESSAGE_RATE_EXCEEDED)
                {
                    //return View("Wait", (object)User);
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
            
        }

        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage PostReminder(Reminder r)
        {
            if (!this.ModelState.IsValid || r == null || !Utils.checkUri(r.urls))
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;

            int _id = PictogramsDb.addReminder(r,username);
            if (_id == -1) return this.Request.CreateResponse(HttpStatusCode.NotFound, "Can´t add reminder to a user that don´t exist");
            HttpResponseMessage response = this.Request.CreateResponse(HttpStatusCode.Created, r);
            response.Headers.Location = new Uri(Url.Link("Default", new { idx = _id , Controller = "Home",Action = "EditReminder" }));

            return response;
        }

        //Testar retirei o contacto
        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage DeleteReminder(int id)
        {
            if (!this.ModelState.IsValid || id<0)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;
            //check if reminder is from user

            Reminder r = PictogramsDb.getReminder(id,username);
            if (r == null)
                throw new HttpResponseException(this.Request.CreateResponse(HttpStatusCode.NotFound));

            PictogramsDb.DeleteReminder(id,username);
            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        
    }
}

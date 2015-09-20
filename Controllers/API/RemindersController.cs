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
        public IEnumerable<Reminder> getAllReminders()
        {

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String [] credentials = idd.Name.Split(new char []{' '});

            //get all reminders that user requested
            IEnumerable<Reminder> list= PictogramsDb.getAllReminders(Int32.Parse(credentials[0]), credentials[1]);
            if (list.Any())
            {
                //filter those reminders
                list = list.Where(r =>  DateTime.ParseExact(r.date+" "+r.time, "yyyy-M-d HH:mm",
                                      System.Globalization.CultureInfo.InvariantCulture) > DateTime.UtcNow.AddHours(1));
                //transfer reminders to historic
                PictogramsDb.TransferReminderstoHistorical(list, Int32.Parse(credentials[0]), credentials[1]);
            }

            return list;
        }


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

       
        [HttpPost]
        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage PostReminder(Reminder r)
        {

            if (!this.ModelState.IsValid || r == null) //|| !Utils.checkUri(r.urls))
                return this.Request.CreateResponse(HttpStatusCode.BadRequest,ModelState.ToString());

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;

            //try to add reminder
            r.daysofweek = Utils.getDaysOfWeekInt(r.repeatingDays);
            int _id = PictogramsDb.addReminder(r,username);
            //contact didnt'exist so reminder wans´t added
            if (_id == -1) return this.Request.CreateResponse(HttpStatusCode.NotFound, "Can´t add reminder to a user that don´t exist");

            //get token used to send notifications by GMC
            String contact = r.contact;
            String reg_id = PictogramsDb.getRegisteredId(contact, username);

            //token didnt'exist
            if (reg_id == String.Empty) return this.Request.CreateResponse(HttpStatusCode.NotFound, "User is not registered in GCM");

            //check if there are reminders ready to synchronize after notification arrived
            int id = PictogramsDb.getContactId(contact, username);
            IEnumerable<Reminder> list = PictogramsDb.getAllReminders(id, username);
            if (!list.Any()) return new HttpResponseMessage(HttpStatusCode.NoContent);

            //Send GCM message
            Sender s = new Sender(Constants.Project_key);
            Message m = new Message.Builder()
                 .collapseKey("Update reminders")
                 .timeToLive(2419200)
                 .delayWhileIdle(true)
                 .dryRun(false)
                 .addData("Email", username)
                 .build();

            Result result = s.sendNoRetry(m, reg_id);
            String errorcode = result.getErrorCodeName();

            if (errorcode != null)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError); 
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
            
        }

        //Testar retirei o contacto
        [HttpDelete]
        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage DeleteReminder(int id)
        {
            if (!this.ModelState.IsValid || id<0)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;

            //check if reminder is from user
            Reminder r = PictogramsDb.getReminder(id,username);
            if (r == null)
                throw new HttpResponseException(this.Request.CreateResponse(HttpStatusCode.NotFound));

            PictogramsDb.DeleteReminder(id,username);
            return new HttpResponseMessage(HttpStatusCode.NoContent); ;
        }

        
    }
}

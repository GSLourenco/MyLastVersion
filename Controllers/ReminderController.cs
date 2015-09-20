using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication2.DataModel;
using MvcApplication2.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Net;
using MvcApplication2.GCM;
using System.Web.Security;
using System.Security.Principal;
using System.Data.SqlClient;
using System.IO;
using Amazon;
using Amazon.S3;


namespace MvcApplication2.Controllers
{
    public class ReminderController : Controller
    {
        //
        // GET: /Reminder/

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ViewResult SelectContact()
        {
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
            List<String> receive_users = PictogramsDb.getUsersFrom(name);
            
            if (!receive_users.Any()) 
                return View("../Contacts/GenerateCode",model:"Não existem contactos na sua lista, adicione");

            if(TempData["shortMessage"]!=null)
                @ViewBag.Message = TempData["shortMessage"].ToString();

            return View("teste", receive_users);
        }


        [Authorize]
        public ActionResult EditReminder(int idx, string username)
        {
            if (!this.ModelState.IsValid || username.Trim() == "" || idx < 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
            
           
            if (Request.IsAjaxRequest())
            {
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                string contact = PictogramsDb.getUserName(Int32.Parse(username));
                bool checkUsername = PictogramsDb.checkIfUserExists(name, contact);

                if (!checkUsername) RedirectToAction("SelectContact", "Reminder") ;
                Reminder r = PictogramsDb.getReminder(idx, name);
                if (r == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                return PartialView("ReminderDetails", r);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

       
        [Authorize, HttpPost]
        public ActionResult EditReminder(Reminder r)
        {

            //get error string if model is not valid or the uris are not uris from our amazon bucket
            String error = "";
            bool ModelError = false; bool UriError = false;
            if ((ModelError = !this.ModelState.IsValid) || r == null)// || (UriError=!Utils.checkUri(r.urls)))
            {
                if (ModelError)
                {
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError e in modelState.Errors)
                        {
                            error += e.ErrorMessage + "<br/>";
                        }
                    }
                }
                if (UriError) error += "Urls de imagem inválidos ou ultrapassou o limite de imagens num lembrete" + "<br/>";
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if(Request.IsAjaxRequest()){
            
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

            //check if the reminder that is being edited is from this user
            bool b = PictogramsDb.checkIfReminderIsFromThisUser(r.id, name);
            if (!b) return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            //edit reminder
            r.daysofweek = Utils.getDaysOfWeekInt(r.repeatingDays);
            PictogramsDb.EditReminder(r);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }


        [Authorize]
        public ActionResult SendReminders(String User)
        {
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
            String reg_id = PictogramsDb.getRegisteredId(User, name);

            //check if user exists or is not registered in GCM
            if (reg_id == String.Empty) {
                TempData["shortMessage"] = "o contacto já não existe, recomeçe o processo de novo";
                return RedirectToAction("SelectContact");
            }   
            int id = PictogramsDb.getContactId(User, name);
            IEnumerable<Reminder> list = PictogramsDb.getAllReminders(id, name);
            if (!list.Any())
            {
                TempData["shortMessage"] = "Não existia lembretes para o contacto que tentou enviar ou o contacto já não existe, recomeçe o processo de novo";
                return RedirectToAction("SelectContact");
            }

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
                return View("Contacts/Error");
            }

            return View();
        }


       

        [Authorize, HttpPost]
        public ActionResult addReminder(Reminder reminder)
        {

            //get error string if model is not valid or the uris are not uris from our amazon bucket
            String error = "";
            bool ModelError=false; bool UriError=false;
            if ((ModelError= !this.ModelState.IsValid) || reminder == null)// || (UriError=!Utils.checkUri(reminder.urls)))
            {
                if (ModelError)
                {
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError e in modelState.Errors)
                        {
                            error += e.ErrorMessage + "<br/>";
                        }
                    }
                }
                if (UriError) error += "Urls de imagem inválidos ou ultrapassou o limite de imagens num lembrete" + "<br/>";
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            

            if (Request.IsAjaxRequest())
            {
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                reminder.daysofweek = Utils.getDaysOfWeekInt(reminder.repeatingDays);
                
                int idx = PictogramsDb.addReminder(reminder, name);
                //cant add reminder because there's no contact
                if (idx == -1) return Json(new { redirectUrl = "SelectContact" }, JsonRequestBehavior.AllowGet) ;
                return Json(new { id = idx,title=reminder.title,contact=PictogramsDb.getContactId(reminder.contact,name)}, JsonRequestBehavior.AllowGet); 
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        //check things
        [Authorize, HttpGet]
        public ActionResult getAllReminders(String user)
        {
            if (!this.ModelState.IsValid || user == String.Empty)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request"); 

            if (Request.IsAjaxRequest())
            {
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

                //chech if contact exists 
                int id = PictogramsDb.getContactId(user, name);
                if(id < 0) 
                    return Json(new { redirectUrl = "SelectContact" }, JsonRequestBehavior.AllowGet);
                //get reminders for that contact
                IEnumerable<Reminder> list = PictogramsDb.getAllReminders(id, name);

                return Json(list, JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        [Authorize, HttpDelete]
        public ActionResult DeleteReminder(int id)
        {
            if (!this.ModelState.IsValid || id < 0)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

            //check if reminder is exists for our user
            Reminder r = PictogramsDb.getReminder(id, name);
            if (r == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            //delete reminder
            PictogramsDb.DeleteReminder(id, name);
            return new HttpStatusCodeResult(HttpStatusCode.NoContent);

        }

        [Authorize, HttpPost]
        public ActionResult UploadFile()
        {

                //check if its uploading only one file
                if (Request.Files.Count != 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                //get file and check if it is an image
                HttpPostedFileBase file = Request.Files[0];
                if (!Program.IsImage(file)) return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType);


                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                String url = null;

                
                if (file != null)
                {
                    //check if user has enough traffic to upload the file
                    if (PictogramsDb.tryUpdateTraffic(name, file.ContentLength))
                    {
                        //upload the file
                        url = Program.putObject(file,name);
                    }
                    else return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                //check if exists a differente file with the same name as the one you were trying to upload
                if (url == null) return new HttpStatusCodeResult(HttpStatusCode.Conflict);
                return Json(url);
        }

        [Authorize, HttpPost]
        public ActionResult ReplaceFile()
        {
                //check if its uploading only one file
                if (Request.Files.Count != 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                //get file and check if it is an image
                HttpPostedFileBase file = Request.Files[0];
                if (!Program.IsImage(file)) return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType);

                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                String url = null;

                
                if (file != null)
                {
                    //check if user has enough traffic to upload the file
                    if (PictogramsDb.tryUpdateTraffic(name, file.ContentLength))
                    {
                        //replace existing image with our file
                        url = Program.ReplaceObject(file,name);
                    }
                    else return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                return Json(url);
            
        }

        [Authorize, HttpGet]
        public ActionResult GetHistorical(String contact)
        {

            if (!this.ModelState.IsValid || contact == String.Empty)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");

            
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                //check if contact exists
                //do ajax code
                int id = PictogramsDb.getContactId(contact, name);
                if (id == -1) return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Contact doesn't exist");

                //get list of reminders that were send in the past
                IEnumerable<Reminder> list = PictogramsDb.GetHistoricalReminders(name, id);

                return View(list);
            
        }

        [Authorize, HttpGet]
        public ActionResult GetHistoricalDetailedReminder(int idx)
        {
            if (!this.ModelState.IsValid || idx < 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");


            if (Request.IsAjaxRequest())
            {
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                
                //check if reminder is exists for our user
                // do ajax xode
                Reminder r = PictogramsDb.getHistoricalReminder(idx, name);
                if (r == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                return PartialView("HistoricReminderDetails", r);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        

    }
}
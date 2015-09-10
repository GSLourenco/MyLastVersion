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

        //check if all fields are necessary in the model
        [Authorize, HttpPost]
        public ActionResult EditReminder(Reminder r)
        {
            if(Request.IsAjaxRequest()){
            if (!this.ModelState.IsValid || r.id < 0 || r == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");

            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
            bool b = PictogramsDb.checkIfReminderIsFromThisUser(r.id, name);
            if (!b) return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

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

            if (reg_id == String.Empty) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User is not registered in GCM");
            int id = PictogramsDb.getContactId(User, name);
            IEnumerable<Reminder> list = PictogramsDb.getAllReminders(id, name);
            if (!list.Any())
            {
                TempData["shortMessage"] = "Não existia lembretes para o contacto que tentou enviar, recomeçe o processo de novo";
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
                // falta fazer as vistas
                //if (errorcode == Constants.ERROR_INVALID_REGISTRATION)
                //{
                //    return View("UnregisterUser", (object)User);
                //}
                //else if (errorcode == Constants.ERROR_NOT_REGISTERED)
                //{
                //    return View("RefreshToken", (object)User);
                //}
                //else if (errorcode == Constants.ERROR_MESSAGE_RATE_EXCEEDED)
                //{
                //    return View("Wait", (object)User);
                //}
            }

            return View();
        }


       

        [Authorize, HttpPost]
        public ActionResult addReminder(Reminder reminder)
        {
            String error = "";
            bool ModelError=false; bool UriError=false;
            if ((ModelError= !this.ModelState.IsValid) || reminder == null || (UriError=!Utils.checkUri(reminder.urls)))
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
                if (UriError) error +="Urls de imagem inválidos";
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            

            if (Request.IsAjaxRequest())
            {
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                reminder.daysofweek = Utils.getDaysOfWeekInt(reminder.repeatingDays);
                //Alterar na bd de reminders
                int idx = PictogramsDb.addReminder(reminder, name);
                //Contacto foi eliminado entretanto
                if (idx == -1) return Json(new { redirectUrl = "SelectContact" }, JsonRequestBehavior.AllowGet); ;
                return Content(idx + " " + reminder.title + " " + PictogramsDb.getContactId(reminder.contact,name));
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
                int id = PictogramsDb.getContactId(user, name);
                if(id < 0) 
                return Json(new { redirectUrl = "SelectContact" }, JsonRequestBehavior.AllowGet);

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

            Reminder r = PictogramsDb.getReminder(id, name);
            if (r == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            PictogramsDb.DeleteReminder(id, name);
            return new HttpStatusCodeResult(HttpStatusCode.NoContent);

        }

        [Authorize, HttpPost]
        public ActionResult UploadFile()
        {

                if (Request.Files.Count != 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                HttpPostedFileBase file = Request.Files[0];
                if (!Program.IsImage(file)) return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType);
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                String url = null;

                if (file != null)
                {
                    if (PictogramsDb.tryUpdateTraffic(name, file.ContentLength))
                    {
                        url = Program.putObject(file,name);
                    }
                    else return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                if (url == null) return new HttpStatusCodeResult(HttpStatusCode.Conflict);
                return Json(url);
            
        }

        [Authorize, HttpPost]
        public ActionResult ReplaceFile()
        {

                if (Request.Files.Count != 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                String url = null;
                HttpPostedFileBase file = Request.Files[0];
                if (!Program.IsImage(file)) return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType);
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

                if (file != null)
                {
                    if (PictogramsDb.tryUpdateTraffic(name, file.ContentLength))
                    {
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
                int id = PictogramsDb.getContactId(contact, name);
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
                
                Reminder r = PictogramsDb.getHistoricalReminder(idx, name);
                if (r == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                return PartialView("HistoricReminderDetails", r);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        

    }
}

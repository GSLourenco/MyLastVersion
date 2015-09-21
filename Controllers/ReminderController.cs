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
            List<String> receive_users = ContactBusinessLayer.GetUsers(name);

            if (!receive_users.Any())
                return View("../Contacts/GenerateCode", model: "Não existem contactos na sua lista, adicione");

            if (TempData["shortMessage"] != null)
                @ViewBag.Message = TempData["shortMessage"].ToString();

            return View("teste", receive_users);
        }


        [Authorize]
        public ActionResult EditReminder(int idx, string username)
        {
            if (!this.ModelState.IsValid || username.Trim() == "")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");


            if (Request.IsAjaxRequest())
            {
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

                Reminder r = ReminderBusinessLayer.GetReminderById(idx, name);
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

            if (Request.IsAjaxRequest())
            {

                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

                //check if the reminder that is being edited is from this user
                int check = ReminderBusinessLayer.EditReminder(r, name);
                if(check==-1) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                if (check == -2)
                {
                    error += "Data inválida, não pode selecionar uma hora passada" + "<br/>";
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }


        [Authorize]
        public ActionResult SendReminders(String User)
        {
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
            String validate = ReminderBusinessLayer.ValidateSendReminders(User, name); 

            if (validate==null){
                TempData["shortMessage"] = "Não existia lembretes para o contacto que tentou enviar ou o contacto já não existe, recomeçe o processo de novo";
                return RedirectToAction("SelectContact");
            }

            String errorcode = ServiceLayer.SendNotification(validate, name);
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
            bool ModelError = false; bool UriError = false;
            if ((ModelError = !this.ModelState.IsValid) || reminder == null)// || (UriError=!Utils.checkUri(reminder.urls)))
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

                int idx = ReminderBusinessLayer.addReminder(reminder, name);
                if (idx == -1) return Json(new { redirectUrl = "SelectContact" }, JsonRequestBehavior.AllowGet);
                if (idx == -2)
                {
                    error += "Data inválida, não pode selecionar uma hora passada" + "<br/>";
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
                }
                return Json(new { id = idx, title = reminder.title, contact = PictogramsDb.getContactId(reminder.contact, name) }, JsonRequestBehavior.AllowGet);
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
                //get reminders for that contact
                IEnumerable<Reminder> list = ReminderBusinessLayer.GetReminders(name, user);
                if (list == null) return Json(new { redirectUrl = "SelectContact" }, JsonRequestBehavior.AllowGet); 

                return Json(list, JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        [Authorize, HttpDelete]
        public ActionResult DeleteReminder(int id)
        {
            if (!this.ModelState.IsValid)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

            //check if reminder  exists for our user
            Boolean delete = ReminderBusinessLayer.DeleteReminder(name, id);
            if (!delete) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            return new HttpStatusCodeResult(HttpStatusCode.NoContent);

        }

        [Authorize, HttpPost]
        public ActionResult UploadFile()
        {

            //check if its uploading only one file
            if (Request.Files.Count != 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //get file and check if it is an image
            HttpPostedFileBase file = Request.Files[0];
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

            HttpStatusCodeResult r = ServiceLayer.UploadFile(file, name);
            if(r.StatusCode == (int)HttpStatusCode.OK)
                return Json(r.StatusDescription);
            return r;
        }

        [Authorize, HttpPost]
        public ActionResult ReplaceFile()
        {
            //check if its uploading only one file
            if (Request.Files.Count != 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            HttpPostedFileBase file = Request.Files[0];
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;

            HttpStatusCodeResult r = ServiceLayer.ReplaceFile(file, name);
            if (r.StatusCode == (int)HttpStatusCode.OK)
                return Json(r.StatusDescription);
            return r;

        }

        [Authorize, HttpGet]
        public ActionResult GetHistorical(String contact)
        {

            if (!this.ModelState.IsValid || contact == String.Empty)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");


            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
            //check if contact exists
            //do ajax code
            IEnumerable<Reminder> list =ReminderBusinessLayer.GetHistoricalReminders(contact,name);
            if (list == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Contact doesn't exist");
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
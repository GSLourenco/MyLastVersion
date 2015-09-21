using System;
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

namespace MvcApplication2.Controllers
{
    public class ContactsController : Controller
    {


        [HttpPost, Authorize]
        public ActionResult GenerateCode(String contact)
        {
            if (!this.ModelState.IsValid || contact.Trim() == "" || contact.Length > 20)
            {
                ModelState.AddModelError("", "O contacto que inseriu não é válido");
                return View();
            }


            //check if user exists
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
            int validate = ContactBusinessLayer.PostContact(contact, name);

            if(validate==0){
                ModelState.AddModelError("", "O contacto que quer adicionar já existe, escolha outro nome");
                return View();
            }
            return RedirectToAction("ManageContacts");

        }

        [HttpGet, Authorize]
        public ActionResult ManageContacts()
        {
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
            List<TemporaryCode> list = ContactBusinessLayer.GetTemporaryCodes(name);

            return View(list);
        }




        [Authorize]
        public ActionResult GenerateCode()
        {
            return View();
        }


        [Authorize, HttpPost]
        public ActionResult DeleteContact(String contact)
        {

            if (!this.ModelState.IsValid || contact == null || contact.Length > 20)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");

            //only accepts ajaxrequest
            if (Request.IsAjaxRequest())
            {
                //delete contact
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                ContactBusinessLayer.DeleteContact(contact, name);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);

        }



    }
}

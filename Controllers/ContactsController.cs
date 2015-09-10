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
        //
        // GET: /Contacts/

        [HttpPost, Authorize]
        public ActionResult GenerateCode(String contact)
        {
            if (!this.ModelState.IsValid || contact.Trim() == ""||contact.Length>20)
            {
                ModelState.AddModelError("", "O contacto que inseriu não é válido");
                return View();
            }

           
                //ver se o contacto já existe
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                int idx = PictogramsDb.getContactId(contact, name);
                if (idx > 0)
                {
                    ModelState.AddModelError("", "O contacto que quer adicionar já existe, escolha outro nome");
                    return View();
                }

                Random rnd = new Random();
                int code = rnd.Next(10000, 99999);
                String status = "Código não inserido";

                while (!PictogramsDb.InsertTemporaryCode(contact, code, status, name))
                {
                    code = rnd.Next(10000, 99999);
                }
             
                return RedirectToAction("ManageContacts");
            
        }

        [HttpGet, Authorize]
        public ActionResult ManageContacts()
        {
            String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
            List<TemporaryCode> list = PictogramsDb.getTemporaryCodes(name);

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

            if (!this.ModelState.IsValid || contact == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");

            if (Request.IsAjaxRequest())
            {
                String name = (HttpContext.User as ICustomPrincipal).Identity.Name;
                bool check = PictogramsDb.DeleteUser(contact, name);
                if (!check) return new HttpStatusCodeResult(HttpStatusCode.NoContent);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);

        }



    }
}
